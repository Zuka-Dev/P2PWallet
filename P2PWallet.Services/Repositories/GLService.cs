using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using P2PWallet.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Repositories
{
    public class GLService : IGLSevice
    {
        private readonly P2PWalletDbContext _context;

        public GLService(P2PWalletDbContext context)
        {
            _context = context;
        }
        public async Task<GeneralLedger> GetOrCreateGL(CreateGLDTO createGLDTO)
        {
            var ledger = await _context.GeneralLedgers.FirstOrDefaultAsync(x=>x.Currency.Equals(createGLDTO.GLCurrency));
            if (ledger is not null)
            {
                return ledger;
            }
            ledger = new GeneralLedger
            {
                GLAccountNo= GenerateAcctNumber.GenerateGLAccountNumber(createGLDTO.GLCurrency),
                Balance=0m,
                Currency=createGLDTO.GLCurrency,
                Description=$"{createGLDTO.GLCurrency} charges"

            };
            await _context.GeneralLedgers.AddAsync(ledger);
            await _context.SaveChangesAsync();
            return ledger;
        }
    }
}
