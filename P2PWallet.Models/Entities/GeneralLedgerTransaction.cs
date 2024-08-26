using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class GeneralLedgerTransaction
    {
        public int Id { get; set; }
        public string GLAccountNo { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime TransactionTime { get; set; }
    }
}
