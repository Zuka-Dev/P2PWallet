using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly P2PWalletDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public AccountRepository(P2PWalletDbContext context, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }
        public async Task<BaseResponseDTO> CreateAccount()
        {
            try
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber);
                if (userIdClaim == null)
                {
                    new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "Missing or invalid user ID in JWT token",
                        Data = new { }
                    };
                }
                var userId = userIdClaim?.Value;
                var user = await _context.Users.FirstOrDefaultAsync(x => Convert.ToString(x.Id) == userId);
                if (user is null)
                {
                    return new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "User Does not exist",
                        Data = new { }
                    };
                }
                var existingNgnAccount = await _context.Accounts.FirstOrDefaultAsync(a => Convert.ToString(a.UserId) == userId && a.Currency == "NGN");
                if (existingNgnAccount != null)
                {
                    return new BaseResponseDTO
                    {
                        Status = false,
                        StatusMessage = "User already has an NGN account",
                        Data = new { }
                    };
                }
                //new Account class.
                var account = new Account
                {
                    AccountNumber = GenerateAcctNumber.GenerateAccountNumber(DateTime.Now),
                    Balance = 0m,
                    Currency = "NGN",
                    UserId = user.Id,
                    CreatedAt = DateTime.Now,

                };
                await _context.Accounts.AddAsync(account);
                await _context.SaveChangesAsync();
                return new BaseResponseDTO
                {
                    Status = true,
                    StatusMessage = "Account Succefully Created",
                    Data = new AccountDTO
                    {
                        AccountNumber = account.AccountNumber,
                        Balance = account.Balance,
                        Currency = account.Currency
                    }
                };
            }
            catch (Exception ex)
            {
                throw;
            }

            //Return Details.

        }

        public async Task<BaseResponseDTO> GetAccountDetails(string accountNumber)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(x => x.AccountNumber == accountNumber);
            if (account is null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "User does not exist",
                    Data = new { }
                };
            }
            return new BaseResponseDTO
            {
                Status = true,
                StatusMessage = "Account Details returned",
                Data = new
                {
                    AccountNumber = account.AccountNumber,
                    Balance = account.Balance,
                    Currency = account.Currency,
                    CreatedAt = account.CreatedAt,
                }
            };


        }

        public async Task<BaseResponseDTO> GetAccountsByUserId(int userId)
        {
            var accounts = await _context.Accounts.Where(acct => acct.UserId == userId).ToListAsync();

            return new BaseResponseDTO
            {
                Status = true,
                StatusMessage = "List of Accounts returned",
                Data = accounts
            };
        }

        public async Task<BaseResponseDTO> TransferMoney(TransferDTO transfer)
        {
            if(transfer.BeneficiaryId == null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Beneficiary Account does not exist",
                    Data = new { }
                };
            }
            if(transfer.Amount <= 0)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Transfer amount must be more than 0 ",
                    Data = new { }
                };
            }
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber);
            if(userIdClaim == null)
            {
                new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Missing or invalid user ID in JWT token",
                    Data = new { }
                };
            }
            var senderId = userIdClaim.Value;
            var sender = await _context.Users.FirstOrDefaultAsync(x => Convert.ToString(x.Id) == senderId);
            var senderAccount = await _context.Accounts.FirstOrDefaultAsync(x => Convert.ToString(x.UserId) == senderId);
            if (!VerifyPin(transfer.Pin, sender.PinHash, sender.PinSalt))
            {
                new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Wrong Pin",
                    Data = new { }
                };
            }
            var beneficiaryAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.AccountNumber == transfer.BeneficiaryId);
            var beneficiaryUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == beneficiaryAccount.UserId);
             if(senderAccount is null)
             {
                 return new BaseResponseDTO
                 {
                     Status = false,
                     StatusMessage = "Sender Account does not exist",
                     Data = new { }
                 };
             }
             if(beneficiaryAccount is null)
             {
                 return new BaseResponseDTO
                 {
                     Status = false,
                     StatusMessage = "Beneficiary Account does not exist",
                     Data = new { }
                 };
             }
            //Get Users involved
            if(beneficiaryAccount == senderAccount)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Invalid Beneficiary",
                    Data = new { }
                };
            }

            if (senderAccount.Balance <= transfer.Amount)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Insufficient Balance",
                    Data = new {}
                };
            }


            senderAccount.Balance -= transfer.Amount;
            beneficiaryAccount.Balance += transfer.Amount;
            var id = new Guid();
        
            await _context.Transfers.AddAsync(new Transfer
            {
                SenderId = senderAccount.AccountNumber,
                BeneficiaryId = transfer.BeneficiaryId,
                Amount = transfer.Amount,
                Id = id
            });
            await _context.SaveChangesAsync();
            //Send Mail
            var creditDTO = new EmailDTO
            {
                Email = beneficiaryUser.Email,
                Balance = beneficiaryAccount.Balance,
                AccountNumber = beneficiaryAccount.AccountNumber,
                Currency = senderAccount.Currency,
                Amount = transfer.Amount,
                FirstName = beneficiaryUser.FirstName,
                LastName = beneficiaryUser.LastName,
                TransactionDate = DateTime.Now.ToString(),
                TransactionID = id.ToString()

            };
            var debitDTO = new EmailDTO
            {
                Email = sender.Email,
                Balance = senderAccount.Balance,
                AccountNumber = senderAccount.AccountNumber,
                Currency = senderAccount.Currency,
                Amount = transfer.Amount,
                FirstName = sender.FirstName,
                LastName = sender.LastName,
                TransactionDate = DateTime.Now.ToString(),
                TransactionID = id.ToString()

            };
            await _emailService.SendDebitEmail(debitDTO);
            await _emailService.SendCreditEmail(creditDTO);
            return new BaseResponseDTO
            {
                Status = true,
                StatusMessage = "Money has been successfully transfered",
                Data = new {}
            };
        }
        private bool VerifyPin(string pin, byte[] pinHash, byte[] pinSalt)
        {
            using (var hmac = new HMACSHA512(pinSalt))
            {
                var inputPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(pin));
                return pinHash.SequenceEqual(inputPassword);
            }
        }



    }
    }

