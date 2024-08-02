using P2PWallet.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interfaces
{
    public interface IUserRepository
    {
        Task<BaseResponseDTO> RegisterUser(CreateUserDTO userDTO);
        Task<BaseResponseDTO> LoginUser(LoginUserDTO userDTO);
        Task<BaseResponseDTO> GetUserDetails();
        Task<bool> CheckUserExists(string email);
        Task<BaseResponseDTO> CreatePin(CreatePinDTO pin);
        Task<BaseResponseDTO> ChangePin(ChangePinDTO pinDTO);
        Task<BaseResponseDTO> ForgotPassword(string email);
        Task<BaseResponseDTO> VerifyToken(VerifyEmailDTO verifyEmailDTO);
        Task<BaseResponseDTO> PasswordReset(ResetPasswordDTO resetPasswordDTO);
        Task<BaseResponseDTO> ChangePassword(PasswordDTO passwordDTO);
        Task<BaseResponseDTO> UpdateUserById(UpdateUserDTO passwordDTO);

        Task<BaseResponseDTO> CreateSecurityQuestions(SecurityQuestionDTO securityQuestionDTO);
        
    }
}
