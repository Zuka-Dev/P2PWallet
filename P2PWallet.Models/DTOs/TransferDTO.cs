using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DTOs
{
    public class TransferDTO
    {
        [Required]
        public string Pin { get; set; }
        [Required]
        public string BeneficiaryId { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }
}
