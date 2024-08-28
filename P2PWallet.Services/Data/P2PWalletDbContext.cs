using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Data
{
    public class P2PWalletDbContext : DbContext
    {
        public P2PWalletDbContext(DbContextOptions options) : base(options)
        {   
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<Deposit> Deposits { get; set; }
        public DbSet<SecurityQuestion> SecurityQuestions  { get; set; }
        public DbSet<SeededSecurityQuestions> SeededSecurityQuestions { get; set;}
        public DbSet<GeneralLedger> GeneralLedgers { get; set; }

        public DbSet<GeneralLedgerTransaction> GeneralLedgerTransactions { get; set; }

        public DbSet<ForeignWalletFundingTransaction> ForeignWalletFundingTransactions { get;set; }
    }
}
