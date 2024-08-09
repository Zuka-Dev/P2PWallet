
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.DTOs;
using P2PWallet.Services.Interfaces;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }
      
        [HttpGet()]
        public async Task<IActionResult> GetAccountsByUserId()
        {
            var obj = await _accountRepository.GetAccountsByUserId();
            if (!obj.Status) return BadRequest(obj);
            return Ok(obj);
        }

        
        [HttpPost("transfer")]
        public async Task<IActionResult> SendMoney([FromBody] TransferDTO transaction)
        {
            var obj = await _accountRepository.TransferMoney(transaction);
            if (!obj.Status) return NotFound(obj);
            return Ok(obj);
        }

        [HttpGet("account/{id}")]
        public async Task<IActionResult> GetAccountDetails([FromRoute] string id)
        {
            var obj = await _accountRepository.GetAccountDetails(id);
            if (!obj.Status) return NotFound(obj);
            return Ok(obj);
        }
        
    }
}
