using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DTOs
{
    public class FundForeignWalletDTO
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Pin {  get; set; }
    }
}
