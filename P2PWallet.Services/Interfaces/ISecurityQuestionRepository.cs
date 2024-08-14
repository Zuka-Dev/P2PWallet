using P2PWallet.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interfaces
{
    public interface ISecurityQuestionRepository
    {
        Task<List<SeededQ>> GetSecurityQuestions();
        Task<SeededQ> GetSecurityQuestionsById(int id);

        Task<BaseResponseDTO> CreateSecurityAnswer(SecurityAnswerDto securityAnswerDto);

        Task<bool> CheckSecurityAnswer(SecurityAnswerCheck securityAnswerCheck);
    }
}
