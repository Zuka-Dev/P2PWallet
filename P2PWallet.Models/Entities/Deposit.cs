using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class Deposit
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Currency { get; set; } = "NGN";
        public string Reference { get; set; }
        public string Status { get; set; } = "Pending";

        [ForeignKey("User")]
        public int userId { get; set; }
        public virtual User User { get; set; }
    }
}
