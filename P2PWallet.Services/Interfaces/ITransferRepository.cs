using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interfaces
{
    public interface ITransferRepository
    {
        Task<IQueryable<Transfer>> GetAllTransfers();
        Task<BaseResponseDTO> GetTransferById(Guid id);
    }
}
