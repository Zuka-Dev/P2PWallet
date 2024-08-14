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
        private readonly ISecurityQuestionRepository _securityQuestionRepository;

        public SecurityQuestionsController(IUserRepository userRepository, ISecurityQuestionRepository securityQuestionRepository)
        {
            _userRepository = userRepository;
            _securityQuestionRepository = securityQuestionRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetSecurityQuestions()
        {
            var sq =  await _securityQuestionRepository.GetSecurityQuestions();
            return Ok(new BaseResponseDTO
            {
                Status = true,
                StatusMessage = "Seed Security Questions Returned",
                Data = sq
            });
        }
        [HttpPost]
        public async Task<IActionResult> CreateSecurityAnswer([FromBody] SecurityAnswerDto securityAnswerDto)
        {
            var secAnswer = await _securityQuestionRepository.CreateSecurityAnswer(securityAnswerDto);
            return Ok(secAnswer);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSecurityQuestionById([FromRoute] int id)
        {
            var question = await _securityQuestionRepository.GetSecurityQuestionsById(id);
            return Ok(new BaseResponseDTO
            {
                Status = true,
                StatusMessage = "Security Question Returned",
                Data = question
            });
        }
        [HttpPost("answer")]
        public async Task<IActionResult> CheckSecurityAnswer([FromBody] SecurityAnswerCheck securityAnswerCheck)
        {
            var obj = await _securityQuestionRepository.CheckSecurityAnswer(securityAnswerCheck);
            if (!obj)
            {
                return BadRequest(new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Wrong Security Answer"
                });
            }
                return Ok(new BaseResponseDTO
                {
                    Status = true,
                    StatusMessage = "Correct Security Answer"
                });
            }
        }


        
    }
