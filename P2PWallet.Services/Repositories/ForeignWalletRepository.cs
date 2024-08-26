using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NPOI.SS.Formula.Eval;
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
    public class ForeignWalletRepository : IForeignWalletRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly P2PWalletDbContext _context;
        private readonly IConfiguration _config;
        private readonly IGLSevice _gLSevice;

        public ForeignWalletRepository(IHttpContextAccessor httpContextAccessor, P2PWalletDbContext context, IConfiguration config, IGLSevice gLSevice)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _config = config;
            _gLSevice = gLSevice;
        }
        public async Task<BaseResponseDTO> CreateForeignWallet(CreateForeignWalletDTo createForeignWalletDTo)
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
            var userAccount = await _context.Accounts.FirstOrDefaultAsync(x => Convert.ToString(x.UserId) == userId && x.Currency == createForeignWalletDTo.Currency);
            if(userAccount is not null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = $"User Already has and account in this currency {createForeignWalletDTo.Currency}"
                };
            }
            //Get Naira Account
            var userNairaAccount = await _context.Accounts.FirstOrDefaultAsync(x => Convert.ToString(x.UserId) == userId && x.Currency == "NGN");
            //Subtract Charges from Naira account
            var creationCharge = _config.GetValue<decimal>($"WalletCreationCharges:{createForeignWalletDTo.Currency}");
            if(userNairaAccount.Balance <= creationCharge)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Insufficient funds"
                };
            }
            var foreignWalletTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                userNairaAccount.Balance = userNairaAccount.Balance - creationCharge;

                var ledger = await _gLSevice.GetOrCreateGL(new CreateGLDTO { GLCurrency = createForeignWalletDTo.Currency });

                //var ledger = await _context.GeneralLedgers.FirstOrDefaultAsync(x => x.Currency == createForeignWalletDTo.Currency && x.Description == $"{createForeignWalletDTo.Currency} charges");
                ledger.Balance += creationCharge;
                var ledgerTransaction = new GeneralLedgerTransaction
                {
                    GLAccountNo = ledger.GLAccountNo,
                    AccountNumber= userNairaAccount.AccountNumber,
                    Currency = "NGN",
                    TransactionTime = DateTime.Now,
                    Amount = creationCharge,
                    Balance = ledger.Balance
                };
                var newAccount = new Account
                {
                    AccountNumber = GenerateAcctNumber.GenerateAccountNumber(DateTime.Now),
                    Balance = 0m,
                    Currency = createForeignWalletDTo.Currency,
                    UserId = Convert.ToInt32(userId),
                    CreatedAt = DateTime.Now,
                };
                await _context.Accounts.AddAsync(newAccount);
                await _context.SaveChangesAsync();
                await foreignWalletTransaction.CommitAsync();
                return new BaseResponseDTO
                {
                    Status = true,
                    StatusMessage = $"{createForeignWalletDTo.Currency} account successfully created"
                };
            }
            catch (Exception ex)
            {
                await foreignWalletTransaction.RollbackAsync();
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = (string)ex.Message,
                };
            }
        }
        
    }
}
