using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DTOs
{
    public class VerifyEmailDTO
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }
}
