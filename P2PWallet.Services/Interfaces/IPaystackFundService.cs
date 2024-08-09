using P2PWallet.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interfaces
{
    public interface IPaystackFundService
    {
        Task<object> InitialisePaystack(PaystackViewDTO fund);
        Task<BaseResponseDTO> Webhook(Object obj);
        Task<BaseResponseDTO> GetAllDeposits();
    }
}
