using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class Transfer
    {
        public Guid Id { get; set; } = new Guid();
        public string SenderId { get; set; }
        public string BeneficiaryId { get; set;}
        public decimal Amount  { get; set;}
        public DateTime TransactionTime { get; set; } = DateTime.Now;


    }
}
