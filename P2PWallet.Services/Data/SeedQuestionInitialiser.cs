using P2PWallet.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Data
{
    public class SeedQuestionInitialiser
    {
        public static void Initialize(P2PWalletDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if the database has already been seeded
            if (context.SeededSecurityQuestions.Any())
            {
                return;   // DB has been seeded
            }

            var securityQuestions = new SeededSecurityQuestions[]
            {
            new SeededSecurityQuestions { SecurityQuestion = "What was the name of your first pet?" },
            new SeededSecurityQuestions { SecurityQuestion = "In which city were you born?" },
            new SeededSecurityQuestions { SecurityQuestion = "What is your mother's maiden name?" },
            new SeededSecurityQuestions { SecurityQuestion = "What was the make of your first car?" },
            new SeededSecurityQuestions { SecurityQuestion = "What is the name of your favorite childhood teacher?" },
            new SeededSecurityQuestions { SecurityQuestion = "What is your favorite book?" },
            new SeededSecurityQuestions { SecurityQuestion = "What is the name of the street you grew up on?" },
            new SeededSecurityQuestions { SecurityQuestion = "What is your favorite movie?" },
            new SeededSecurityQuestions { SecurityQuestion = "What was the first concert you attended?" },
            new SeededSecurityQuestions { SecurityQuestion = "What is the name of your favorite childhood friend?" }
            };

            foreach (SeededSecurityQuestions q in securityQuestions)
            {
                context.SeededSecurityQuestions.Add(q);
            }
            context.SaveChanges();
        }
    }
}
