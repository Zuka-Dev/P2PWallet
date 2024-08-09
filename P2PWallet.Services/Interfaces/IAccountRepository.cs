using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interfaces
{
    public interface IAccountRepository
    {
        Task<BaseResponseDTO> GetAccountsByUserId();
        Task<bool> CreateAccount(User user);
        Task<BaseResponseDTO> GetAccountDetails(string accountNumber);
        Task<BaseResponseDTO> TransferMoney(TransferDTO transaction);

        

    }
}
