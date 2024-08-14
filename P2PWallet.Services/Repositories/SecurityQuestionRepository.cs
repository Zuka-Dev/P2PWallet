using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Data;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Repositories
{
    public class SecurityQuestionRepository : ISecurityQuestionRepository
    {
        private readonly P2PWalletDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SecurityQuestionRepository(P2PWalletDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> CheckSecurityAnswer(SecurityAnswerCheck securityAnswerCheck)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber);

            var userId = userIdClaim?.Value;

            //Get Security Answer
            var securityAnswer = await _context.SecurityQuestions.FirstOrDefaultAsync(x=> Convert.ToString(x.UserId) == userId);

            return VerifySecurityAnswer(securityAnswerCheck.SecurityA, securityAnswer.AnswerSalt, securityAnswer.AnswerHash);

        }

        public async Task<BaseResponseDTO> CreateSecurityAnswer(SecurityAnswerDto securityAnswerDto)
        {
            //Get User Id
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.SerialNumber);
   
            var userId = userIdClaim?.Value;
            var question = await _context.SecurityQuestions.FirstOrDefaultAsync(x=> Convert.ToString(x.UserId) == userId);
            if(question is not null)
            {
                return new BaseResponseDTO
                {
                    Status = false,
                    StatusMessage = "Security question already created"
                };
            }
            //Hash answer
            HashSecurityAnswer(securityAnswerDto.SecurityA, out byte[] answerSalt, out byte[] answerHash);
            // Map Answer
            var securityQuestion = new SecurityQuestion {
                AnswerHash = answerHash,
                AnswerSalt = answerSalt,
                SeedSecurityQuestionId = securityAnswerDto.SecurityQuestionId,
                UserId = Convert.ToInt32(userId)
            };
            //StoreAnswer
            await _context.SecurityQuestions.AddAsync(securityQuestion);
            await _context.SaveChangesAsync();
            return new BaseResponseDTO { 
                Status=true,
                StatusMessage="Created new security answer"
            };
        }


        public async Task<List<SeededQ>> GetSecurityQuestions()
        {
            List<SeededQ> questions = new List<SeededQ>();

            try
            {
                var data = await _context.SeededSecurityQuestions.ToListAsync();

                data.ForEach(question => questions.Add(new SeededQ
                {
                    Id = question.Id,
                    SecurityQuestion = question.SecurityQuestion

                }));
                return questions;

            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return questions;
            }
        }

        public async Task<SeededQ> GetSecurityQuestionsById(int id)
        {
            var question = await _context.SeededSecurityQuestions.FirstOrDefaultAsync(question => question.Id == id);
            return new SeededQ
            {
                 SecurityQuestion = question.SecurityQuestion,
                 Id = id
            };
        }


        private void HashSecurityAnswer(string answer, out byte[] answerSalt, out byte[] answerHash) {
            using (var hmac = new HMACSHA512()){
                answerSalt = hmac.Key;
                answerHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(answer));
            }
        }
        private bool VerifySecurityAnswer(string answer, byte[] answerSalt,  byte[] answerHash)
        {
            using(var hmac = new HMACSHA512(answerSalt))
            {
                var hashedAnswer = hmac.ComputeHash(Encoding.UTF8.GetBytes(answer));
                return answerHash.SequenceEqual(hashedAnswer);

            }
        }
    }
}
