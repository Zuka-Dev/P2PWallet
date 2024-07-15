using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Utilities;

namespace P2PWallet.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly ITransferRepository _transferRepository;

        public TransferController(ITransferRepository transferRepository)
        {
            _transferRepository = transferRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllTransactions([FromQuery] TransferFilter query)
        {
            IQueryable<Transfer> transfers = await _transferRepository.GetAllTransfers();
         
            
            if (query.Year.HasValue)
            {
                transfers = transfers.Where(t => t.TransactionTime.Year == query.Year);

                if (query.Month.HasValue)
                {
                    transfers = transfers.Where(t => t.TransactionTime.Month == query.Month);
                }
            }


            return Ok(new BaseResponseDTO
            {
                Status = true,
                StatusMessage = "All transaction returned",
                Data = transfers.ToList()
            });
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetTransactionById([FromRoute] Guid id)
        {
            var obj = await _transferRepository.GetTransferById(id);
            return Ok(obj);
        }
    }
}
