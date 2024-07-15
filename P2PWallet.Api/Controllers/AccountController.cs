
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
      
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAccountsByUserId([FromRoute] int id)
        {
            var obj = await _accountRepository.GetAccountsByUserId(id);
            if (!obj.Status) return BadRequest(obj);
            return Ok(obj);
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAccount()
        {
            var obj = await _accountRepository.CreateAccount();
            if (!obj.Status) return NotFound(obj);
            return CreatedAtAction(nameof(CreateAccount), obj);
        }
          [HttpPost("transfer")]
        public async Task<IActionResult> SendMoney([FromBody] TransferDTO transaction)
        {
            var obj = await _accountRepository.TransferMoney(transaction);
            if (!obj.Status) return NotFound(obj);
            return CreatedAtAction(nameof(CreateAccount), obj);
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
