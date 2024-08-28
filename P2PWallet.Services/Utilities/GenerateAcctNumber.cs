using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Utilities
{
    public class GenerateAcctNumber
    {
        public static string GenerateAccountNumber( DateTime dateTime)
        {
            var accountNumber = dateTime.ToString("yy/MM/dd/mm/ss").Replace("/", "");
            // Ensure the account number is 10 digits long
            if (accountNumber.Length > 10)
            {
                throw new ArgumentOutOfRangeException(nameof(accountNumber), "ID cannot exceed 9999 ");
            }

            return accountNumber;
        }
        public static string GenerateGLAccountNumber(string currency)
        {
            const string src = "0123456789";
            int length2 = 4;
            var sb1 = new StringBuilder();
            var sb2 = new StringBuilder();
            Random RNG = new Random();
            for (var i = 0; i < 1; i++)
            {
                sb1.Append("00");
                var c = src[RNG.Next(0, src.Length)];
                sb1.Append(c);
            }            
            for (var i = 0; i < length2; i++)
            {
                var c = src[RNG.Next(0, src.Length)];
                sb2.Append(c);
            }
            return $"{sb1.ToString()}{currency}{sb2.ToString()}";
        }

    }
}
