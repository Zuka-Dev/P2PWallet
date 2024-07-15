using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using P2PWallet.Models.DTOs;
using P2PWallet.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepositController : ControllerBase
    {
        private readonly IPaystackFundService _paystackFundService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;

        public DepositController(IPaystackFundService paystackFundService, IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _paystackFundService = paystackFundService;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> InitializePaystack([FromBody]PaystackViewDTO fund)
        {
            var obj = await _paystackFundService.InitialisePaystack(fund);
           // if(!obj.status) return BadRequest(obj);
            return Ok(obj);
        }
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] Object obj)
        {
            string[] allowedIPs = _config.GetSection("Paystack:AllowedIPs").Get<string[]>();
            string requestIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            if (!allowedIPs.Contains(requestIpAddress))
            {
                return Unauthorized(new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = $"Unauthorised request from IP Address: {requestIpAddress}",
                    Data = new { }
                });
            }
            //Signature Validation
            string secret = _config.GetSection("Paystack:Secret").Value;
            var reqHeader = _httpContextAccessor.HttpContext.Request.Headers;
            String result = "";

            result = GenerateHash(obj.ToString(), secret);

            reqHeader.TryGetValue("x-paystack-signature", out StringValues xpaystackSignature);
            if (xpaystackSignature != result)
            {
                return Unauthorized(new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Unauthorised: Signature does not match",
                    Data = new { }
                });
            }

             Task.Run(async () =>
            {
                await _paystackFundService.Webhook(obj);
            }
            );
            return Ok();
        }
        private string GenerateHash(string requestBody, string webhookSecret)
        {
            var secretBytes = Encoding.UTF8.GetBytes(webhookSecret);
            var payloadBytes = Encoding.UTF8.GetBytes(requestBody);

            using (var hmac = new HMACSHA512(secretBytes))
            {
                var hash = hmac.ComputeHash(payloadBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        //Immediate solution Stop Awaiting.
        //Bring out IPfiltering and Signature validation
        //Check Task Scheduling
    }
}
