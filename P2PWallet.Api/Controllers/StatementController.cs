using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.DTOs;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System.Security.Claims;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatementController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IStatementService _statementService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly P2PWalletDbContext _context;

        public StatementController(IEmailService emailService, IStatementService statementService, IHttpContextAccessor httpContextAccessor, P2PWalletDbContext context)
        {
            _emailService = emailService;
            _statementService = statementService;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateStatement([FromBody] StatementRequestDTO request)
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
            var userId = userIdClaim.Value;
            var user = await _context.Users.Include(x => x.Accounts).FirstOrDefaultAsync(x => Convert.ToString(x.Id) == userId);
            var statementBytes = await _statementService.GenerateStatement(request);
            var fileName = $"Statement_{request.StartDate:yyyyMMdd}_{request.EndDate:yyyyMMdd}.{request.Format}";
            var contentType = request.Format.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            // TODO: Implement logic to send email with attachment
            Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailWithAttachment(
                        user.Email,
                        "Your Statement",
                        "Please find your statement attached.",
                        fileName,
                        statementBytes,
                        contentType);
                }
                catch (Exception ex)
                {
                    // Handle or log any exceptions that occur while sending the email
                    // For example, log the exception
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }
            });
            return File(statementBytes, "application/pdf", "statement.pdf");
        }
    }
}
