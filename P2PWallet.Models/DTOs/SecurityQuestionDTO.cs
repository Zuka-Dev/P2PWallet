using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DTOs
{
    public class SecurityQuestionDTO
    {   
        public List<SecurityQuesDTO> securityQuestions {  get; set; }
    }
    public class SecurityQuesDTO
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
    public class SecurityAnswer
    {
        public int QuestionId { get; set; }
        public string Answer { get;set; }
    }
}
