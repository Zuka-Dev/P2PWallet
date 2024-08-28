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
using System.Security.Cryptography;
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
                    StatusMessage = $"Insufficient funds to create this {createForeignWalletDTo.Currency} account"
                };
            }
            var foreignWalletTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                userNairaAccount.Balance = userNairaAccount.Balance - creationCharge;

                var ledger = await _gLSevice.GetOrCreateGL(new CreateGLDTO {Purpose=GLConstants.Purpose.WalletCreationCharges, GLCurrency = createForeignWalletDTo.Currency });

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
                await _context.GeneralLedgerTransactions.AddAsync(ledgerTransaction);
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

        public async Task<BaseResponseDTO> FundForeignWallet(FundForeignWalletDTO fundForeignWalletDTO)
        {
            /*
             - Get user naira account
             - Get amount to be funded
             - Confirm Pin
             - Rates
             - Begin a transaction with the db
             - Subtract money 
             - Create gl
             - Add money to gl
             - get the currency gl and take money and add to the currency account
             - Register All GL Transactions
             - Return Successfull
             - End transaction
             - *Send credit/Debit Alert
             */
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
            var userId = Convert.ToInt32(userIdClaim.Value);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var userNairaAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.UserId== userId && x.Currency == "NGN");
            var userForeignAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.UserId == userId && x.Currency == fundForeignWalletDTO.Currency);
            if (!VerifyPin(fundForeignWalletDTO.Pin, user.PinHash, user.PinSalt))
            {
                return new BaseResponseDTO { Status = false, StatusMessage = "Incorrect Pin" };
            }
            decimal rate = GetConversionRates(fundForeignWalletDTO.Currency);
            
            decimal amount = fundForeignWalletDTO.Amount * rate;
            if(userNairaAccount.Balance <= amount)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Insufficient Funds"
                };
            }
            var dbTransaction = _context.Database.BeginTransaction();
            try
            {
                userNairaAccount.Balance = userNairaAccount.Balance - amount;
                //Create GL(Naira payment funding GL)
                var ledger = await _gLSevice.GetOrCreateGL(new CreateGLDTO
                {
                    Purpose=GLConstants.Purpose.NairaFundingWallet,
                    GLCurrency="NGN"
                });
                ledger.Balance += amount;
                await _context.GeneralLedgerTransactions.AddAsync(new GeneralLedgerTransaction
                {
                    GLAccountNo = ledger.GLAccountNo,
                    AccountNumber = userNairaAccount.AccountNumber,
                    Currency = "NGN",
                    TransactionTime = DateTime.Now,
                    Amount = amount,
                    Balance = ledger.Balance
                });
                //Create foreign ledger
                var foreignLedger = await _gLSevice.GetOrCreateGL(new CreateGLDTO
                {
                    Purpose = GLConstants.Purpose.ForeignWalletFunder,
                    GLCurrency = fundForeignWalletDTO.Currency
                });
                foreignLedger.Balance -= fundForeignWalletDTO.Amount;
                
                await _context.GeneralLedgerTransactions.AddAsync(new GeneralLedgerTransaction
                {
                    GLAccountNo = foreignLedger.GLAccountNo,
                    AccountNumber = userForeignAccount.AccountNumber,
                    Currency = fundForeignWalletDTO.Currency,
                    TransactionTime = DateTime.Now,
                    Amount = fundForeignWalletDTO.Amount,
                    Balance = foreignLedger.Balance
                });
                userForeignAccount.Balance += fundForeignWalletDTO.Amount;
                await _context.ForeignWalletFundingTransactions.AddAsync(new ForeignWalletFundingTransaction
                {
                    TransactionTime=DateTime.Now,
                    BeneficiaryAccountNumber = userForeignAccount.AccountNumber,
                    Amount = fundForeignWalletDTO.Amount,
                    ExchangeRate = rate,
                    Currency = fundForeignWalletDTO.Currency

                });
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return new BaseResponseDTO
                {
                    Status = true,
                    StatusMessage = $"{fundForeignWalletDTO.Currency} account successfully funded"
                };
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = ex.Message,
                };
                throw;
            }

        }
        private bool VerifyPin(string pin, byte[] pinHash, byte[] pinSalt)
        {
            using (var hmac = new HMACSHA512(pinSalt))
            {
                var inputPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(pin));
                return pinHash.SequenceEqual(inputPassword);
            }
        }
        private decimal GetConversionRates(string currency)
        {
            var conversionRates = _config.GetValue<ConversionRate[]>($"CurrencyConversionRates");
            var _ =  conversionRates.FirstOrDefault(x=>x.currency == currency);
            return _.rate;
        }
        private record ConversionRate
        {
            public string currency = string.Empty;
            public decimal rate;
        }
    }
}
