using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using P2PWallet.Models.DTOs;
using P2PWallet.Services.Interfaces;
using System.Net;
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
        private readonly ILogger _logger;

        public DepositController(IPaystackFundService paystackFundService, IHttpContextAccessor httpContextAccessor, IConfiguration config, ILogger logger)
        {
            _paystackFundService = paystackFundService;
            _httpContextAccessor = httpContextAccessor;
            _config = config;
            _logger = logger;
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
        public async Task<IActionResult> Webhook([FromBody] object obj)
        {
            // Signature Validation
            string secret = _config.GetSection("Paystack:Secret").Value;
            var reqHeader = _httpContextAccessor.HttpContext.Request.Headers;
            reqHeader.TryGetValue("x-paystack-signature", out StringValues xpaystackSignature);
            string calculatedSignature = GenerateHash(obj.ToString(), secret);
            if (xpaystackSignature != calculatedSignature)
            {
                return Unauthorized(new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Unauthorised: Signature does not match",
                    Data = new { }
                });
            }

            // Return 200 OK immediately
            _ = ProcessWebhookAsync(obj); // Fire and forget

            return Ok();
        }
        [HttpGet()]
        public async Task<IActionResult> GetAllDeposits()
        {
            var obj = await _paystackFundService.GetAllDeposits();
            return Ok(obj);
        }

        private async Task ProcessWebhookAsync(object obj)
        {
            try
            {
                var result = await _paystackFundService.Webhook(obj);
                if (result is BaseResponseDTO responseDto && !responseDto.Status)
                {
                    // Log the error or handle it as needed
                    _logger.LogError($"Webhook processing failed: {responseDto.StatusMessage}");
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions
                _logger.LogError($"Error processing webhook: {ex.Message}");
            }
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
