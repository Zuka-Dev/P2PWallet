using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Interfaces;

namespace P2PWallet.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserAuthController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserDTO newUser)
        {
            var obj = await _userRepository.RegisterUser(newUser);
            return CreatedAtAction(nameof(Register), obj);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO newUser)
        {
            var obj = await _userRepository.LoginUser(newUser);
            if (!obj.Status) return NotFound(obj);
            return Ok(obj);
        }


        [HttpGet("user/{email}")]
        public async Task<IActionResult> CheckUserExists([FromRoute] string email)
        {
            var obj = await _userRepository.CheckUserExists(email);

            return Ok(new BaseResponseDTO
            {
                Status = obj,
                StatusMessage = "User Exists",
                Data = new { }
            });
        }

        [HttpPost("user/forgotPassword/{email}")]
        public async Task<IActionResult> ForgotPassword([FromRoute] string email)
        {
            var obj = await _userRepository.ForgotPassword(email);
            return Ok(obj);
        }
        [HttpPost("user/resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
        {
            var obj = await _userRepository.PasswordReset(resetPasswordDTO);
            return Ok(obj);
        }

        [Authorize]
        [HttpPost("user/createPin")]
        public async Task<IActionResult> CreatePin([FromBody] CreatePinDTO pin)
        {
            var obj = await _userRepository.CreatePin(pin);
            return Ok(obj);
        }
        [Authorize]
        [HttpPost("user/changePin")]
        public async Task<IActionResult> ChangePin([FromBody] ChangePinDTO pinDto)
        {
            var obj = await _userRepository.ChangePin(pinDto);
            return Ok(obj);
        }
        [Authorize]
        [HttpPost("user/changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordDTO passwordDTO)
        {
            var obj = await _userRepository.ChangePassword(passwordDTO);
            return Ok(obj);
        }

        [Authorize]
        [HttpGet("user/details/{id:int}")]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            var obj = await _userRepository.GetUserById(id);
            if (!obj.Status) return NotFound(obj);
            return Ok(obj);

        }
        [Authorize]
        [HttpPut("user/detail")]
        public async Task<IActionResult> UpdateUserById([FromBody] UpdateUserDTO updateUserDTO)
        {
            var obj = await _userRepository.UpdateUserById(updateUserDTO);
            if (!obj.Status) return NotFound(obj);
            return Ok(obj);

        }

    }
}
