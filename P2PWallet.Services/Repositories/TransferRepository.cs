using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Repositories
{
    public class TransferRepository : ITransferRepository
    {
        private readonly P2PWalletDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TransferRepository(P2PWalletDbContext context,IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<Transfer>> GetAllTransfers()
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber).Value;
                var user = await _context.Users.Include(x => x.Accounts).FirstOrDefaultAsync(x=>Convert.ToString(x.Id) == userId);
                if (user.Accounts.Count == 0) return null;
                var acctNum = user.Accounts.FirstOrDefault(x => x.Currency == "NGN").AccountNumber;
                var transfers = await _context.Transfers.Where(x => x.SenderId == acctNum || x.BeneficiaryId == acctNum).ToListAsync();
                return transfers;
                
            }catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<BaseResponseDTO> GetTransferById(Guid id)
        {
            try
            {
            var transfer = await _context.Transfers.FirstOrDefaultAsync(x=> x.Id == id);
                return new BaseResponseDTO
                {
                    Status = true,
                    StatusMessage= $"Transfer #{id}",
                    Data= transfer
                };

            }catch(Exception ex)
            {
                throw;
            }
        }
    }
}
