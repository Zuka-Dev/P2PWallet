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
            const string src = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int length = 7;
            var sb = new StringBuilder();
            Random RNG = new Random();
            for (var i = 0; i < length; i++)
            {
                var c = src[RNG.Next(0, src.Length)];
                sb.Append(c);
            }
            return $"{sb.ToString()}${currency}";
        }

    }
}
