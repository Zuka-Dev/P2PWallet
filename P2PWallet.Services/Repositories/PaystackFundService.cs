using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace P2PWallet.Services.Repositories
{
    public class PaystackFundService : IPaystackFundService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly P2PWalletDbContext _context;
        private readonly IEmailService _emailService;
        private HttpClient _httpClient = new HttpClient();
        HttpRequestMessage request;
        HttpResponseMessage response;

        public PaystackFundService(IConfiguration config, IHttpContextAccessor httpContextAccessor, P2PWalletDbContext context,IEmailService emailService)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _emailService = emailService;
        }

        public async Task<object> InitialisePaystack(PaystackViewDTO amount)
        {
            var fund = amount.Fund;
            try
            {
                if(_httpContextAccessor.HttpContext is null)
                {
                    new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "Missing or invalid user ID in JWT token",
                        Data = new { }
                    };
                }
                var userId = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber).Value;
                var user = await _context.Users.FirstOrDefaultAsync(c => Convert.ToString(c.Id) == userId);
                if (user is null)
                {
                    return new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "User not found",
                        Data = new { }
                    };
                }
                /*
                 * Amount
                 * Email
                 * Reference
                 * currency
                 */
                var paystackUrl = System.Text.Encoding.UTF8.GetBytes(_config.GetSection("Paystack:URL").Value);
                string urlString = Encoding.UTF8.GetString(paystackUrl);
                var key = System.Text.Encoding.UTF8.GetBytes(_config.GetSection("Paystack:Secret").Value);
                string keyString = Encoding.UTF8.GetString(key);



                var paystackFund = new PaystackFundDTO
                {
                    amount = Convert.ToString(fund * 100m),
                    email = user.Email,
                    reference = GenerateReference()
                };

                request = new HttpRequestMessage(HttpMethod.Post, urlString);
                string jsonContent = JsonConvert.SerializeObject(paystackFund);

                var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                request.Content = stringContent;
                // Set Bearer token in Authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", keyString);

                response = await _httpClient.SendAsync(request);
                string responsebody = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<PaystackResponseDTO>(responsebody);
                var deposit = new Deposit
                {
                    Amount= fund,
                    userId = user.Id,
                    Reference = responseData.data.reference
                };
                await _context.Deposits.AddAsync(deposit);
                await _context.SaveChangesAsync();
                if (responseData.status)
                {
                    return responseData;
                }
                else
                {
                    Console.WriteLine("Error: " + response.StatusCode);
                    return new BaseResponseDTO
                    {
                        Data = new
                        {
                            Status = false,
                            StatusMessage =response.ToString(),
                            data = new {}
                        }
                    };
                }
                
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //NG ROCK
        public async Task<object> Webhook(Object obj)
        {
            
            try
            {

                //Deserialise the jsonData
                var jsonData = JsonConvert.DeserializeObject<WebhookDTO>(obj.ToString());
                //Check for a user based on the reference and status from Deposit table
                var deposit = await _context.Deposits.FirstOrDefaultAsync(x => x.Reference == jsonData.Data.Reference && x.Status == "Pending");
                //Edit the balance of the users account
                if (deposit is null)
                {
                    return new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "Deposit doesnt exist",
                        Data = new { }
                    };
                }
                var userAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.UserId == deposit.userId && x.Currency == deposit.Currency);
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userAccount.UserId);
                if (userAccount is null)
                {
                    return new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "Account doesnt exist",
                        Data = new { }
                    };
                }
                userAccount.Balance += deposit.Amount;

                // Change status
                deposit.Status = "Successful";
                // Add user.
                await _context.SaveChangesAsync();

                var creditDTO = new EmailDTO
                {
                    Email = user.Email,
                    Balance = userAccount.Balance,
                    AccountNumber = userAccount.AccountNumber,
                    Currency = userAccount.Currency,
                    Amount = deposit.Amount,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    TransactionDate = deposit.Date.ToString(),
                    TransactionID = deposit.Id.ToString(),

                };
                await _emailService.SendCreditEmail(creditDTO);
                return new OkResult();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private static string GenerateReference()
        {
            const string src = "abcdefghijklmnopqrstuvwxyz0123456789";
            int length = 16; 
            var sb = new StringBuilder();
            Random RNG = new Random();
            for (var i = 0; i < length; i++)
            {
                var c = src[RNG.Next(0, src.Length)];
                sb.Append(c);
            }
            return sb.ToString();
        }
       
    }

}
