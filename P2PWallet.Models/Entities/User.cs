using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }  
        public byte[]? PinHash { get; set; }
        public byte[]? PinSalt { get; set; }

        public string Address { get; set; }
        public byte[]? ImageBase64Byte { get; set; }
        public string? VerificationToken { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? PasswordResetToken { get; set; }
        
        public DateTime? ResetTokenExpires { get; set; }

        public bool? IsVerified { get; set; } = false;
        public virtual SecurityQuestion? SecurityQuestion { get; set; }
        public List<Account> Accounts { get; set; } = new List<Account>();

    }
}
