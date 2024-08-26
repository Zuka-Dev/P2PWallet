using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.DTOs;
using P2PWallet.Services.Interfaces;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ForeignWalletController : ControllerBase
    {
        private readonly IForeignWalletRepository _foreignWalletRepository;

        public ForeignWalletController(IForeignWalletRepository foreignWalletRepository)
        {
            _foreignWalletRepository = foreignWalletRepository;
        }

        [HttpPost("create")]
        public async Task<IActionResult> GenerateForeignWallet(CreateForeignWalletDTo createForeignWalletDTo)
        {
            var obj = await _foreignWalletRepository.CreateForeignWallet(createForeignWalletDTo);
            if (obj == null)
            {
                return NotFound();
            }
            return Ok(obj);
        }
    }
}
