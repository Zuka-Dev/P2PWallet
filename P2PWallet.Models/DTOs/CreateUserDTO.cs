using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DTOs
{
    public class CreateUserDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
     //   public string Pin { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

    }
    public class CreatePinDTO
    {
        public string Pin { get; set; }
    } 
    public class ChangePinDTO
    {
        public string OldPin {  get; set; }
        public string NewPin { get; set; }
        public List<SecurityAnswer> SecurityAnswers { get; set; }
    }
}
