using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DTOs
{
    public class PasswordDTO
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
    public class ForgotPassword
    {

    }
    public class ResetPasswordDTO
    {
        [Required]
        public string? Password { get; set; }
        [Compare("Password",ErrorMessage="Pasword dont match.")]
        public string? ConfirmPassword { get; set; }
        public string? ResetToken { get; set; }
        public string? Email { get; set; }
    }
}
