using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendCreditEmail(EmailDTO emailDTO);
        Task SendDebitEmail(EmailDTO emailDTO);
        Task SendResetTokenEmail(User user,string url);
        Task SendVerificationEmail(User user, string url);
    }
}
