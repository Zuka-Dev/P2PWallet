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
                .Matches(@"[@*""\-+$£']").WithMessage("Password must contain at least one special character.");
            //Dont include whitespace
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .MinimumLength(11).WithMessage("Phone number must be more than 14 digits")
                .MaximumLength(14).WithMessage("Phone number must be less than 14 digits");
                
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
