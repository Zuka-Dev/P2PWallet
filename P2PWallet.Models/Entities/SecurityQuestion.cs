using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Models.Entities
{
    public class SecurityQuestion
    {
        public int Id { get; set; }
        public byte[] AnswerSalt { get; set; }
        public byte[] AnswerHash { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("SeededSecurityQuestions")]
        public int SeedSecurityQuestionId {  get; set; }

        public virtual User User { get; set; }
        public virtual SeededSecurityQuestions SeededSecurityQuestions { get; set; }
    }
}
