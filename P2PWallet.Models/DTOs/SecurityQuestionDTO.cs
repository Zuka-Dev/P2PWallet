using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.DTOs
{
    public class SeededQ
    {
        public int Id { get; set; }
        public string SecurityQuestion { get; set; }
    }


    public class SecurityAnswerDto
    {
        public int SecurityQuestionId { get; set; }
        public string SecurityA { get; set; }
    }
    public class SecurityAnswerCheck
    {
        public int SecurityQuestionId { get; }
        public string SecurityA { get; set; }
    }

}
