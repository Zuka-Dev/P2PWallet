using FluentValidation;
using P2PWallet.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Validators
{
    public class UserDTOValidator : AbstractValidator<CreateUserDTO>
    {
        public UserDTOValidator()
        {
            RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid Email format");
            RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
            RuleFor(x=>x.Password)
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
            //Dont include whitespace

        }
    }
    public class PinDTOValidator : AbstractValidator<CreatePinDTO>
    {
        public PinDTOValidator()
        {
            RuleFor(x=>x.Pin)
                .NotEmpty().WithMessage("Pin is required")
                .Length(4).WithMessage("Pin must be 4 digits")
                .Matches("^[0-9]{6}$").WithMessage("PIN must contain only digits.");


        }
    }
}
