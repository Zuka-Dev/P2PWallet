using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class ForeignWalletFundingTransaction
    {
        public int Id { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime TransactionTime { get; set; }
    }
}
