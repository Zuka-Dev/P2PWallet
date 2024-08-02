using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P2PWallet.Models.DTOs;
using P2PWallet.Services.Interfaces;

namespace P2PWallet.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityQuestionsController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public SecurityQuestionsController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        [HttpPost]
        public async Task<IActionResult> CreateSecurityQuestions(SecurityQuestionDTO securityQuestionDTO)
        {
            var obj = await _userRepository.CreateSecurityQuestions(securityQuestionDTO);
            if (!obj.Status)
            {
                return BadRequest(obj);
            }
            return Ok(obj);
        }
    }
}
