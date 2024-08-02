using Microsoft.Extensions.Configuration;
using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace P2PWallet.Services.Repositories
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {

            _config = config;
        }
        public async Task SendCreditEmail(EmailDTO emailDTO)
        {
            var emailTemplate = await LoadEmailTemplate(_config.GetSection("EmailTemplates:CreditTemplatePath").Value);
            emailTemplate = emailTemplate.Replace("[[Amount]]", emailDTO.Amount.ToString());
            emailTemplate = emailTemplate.Replace("[[Balance]]", emailDTO.Balance.ToString());
            emailTemplate = emailTemplate.Replace("[[Date]]", emailDTO.TransactionDate.ToString());
            emailTemplate = emailTemplate.Replace("[[TransactionID]]", emailDTO.TransactionID.ToString());
            emailTemplate = emailTemplate.Replace("[[AccountNumber]]", MaskAccountNumber(emailDTO.AccountNumber.ToString()));
            emailTemplate = emailTemplate.Replace("[[Currency]]", emailDTO.Currency.ToString());
            emailTemplate = emailTemplate.Replace("[[FirstName]]", emailDTO.FirstName.ToString());
            emailTemplate = emailTemplate.Replace("[[LastName]]", emailDTO.LastName.ToString());
            await SendEmail(emailDTO.Email, "Credit Notification", emailTemplate);

        }

        public async Task SendDebitEmail(EmailDTO emailDTO)
        {
            var emailTemplate = await LoadEmailTemplate(_config.GetSection("EmailTemplates:DebitTemplatePath").Value);
            emailTemplate = emailTemplate.Replace("[[Amount]]", emailDTO.Amount.ToString());
            emailTemplate = emailTemplate.Replace("[[Balance]]", emailDTO.Balance.ToString());
            emailTemplate = emailTemplate.Replace("[[Date]]", emailDTO.TransactionDate.ToString());
            emailTemplate = emailTemplate.Replace("[[TransactionID]]", emailDTO.TransactionID.ToString());
            emailTemplate = emailTemplate.Replace("[[AccountNumber]]", MaskAccountNumber(emailDTO.AccountNumber.ToString()));
            emailTemplate = emailTemplate.Replace("[[Currency]]", emailDTO.Currency.ToString());
            emailTemplate = emailTemplate.Replace("[[FirstName]]", emailDTO.FirstName.ToString());
            emailTemplate = emailTemplate.Replace("[[LastName]]", emailDTO.LastName.ToString());
            await SendEmail(emailDTO.Email, "Debit Notification", emailTemplate);
        }
        public async Task SendResetTokenEmail(User user, string url)
        {
            var emailTemplate = await LoadEmailTemplate(_config.GetSection("EmailTemplates:ResetTokenTemplatePath").Value);
            emailTemplate = emailTemplate.Replace("[[FirstName]]", user.FirstName);
            emailTemplate = emailTemplate.Replace("[[ResetLink]]", url);
            await SendEmail(user.Email, "Password Reset", emailTemplate);
      
        }
        public async Task SendVerificationEmail(User user, string url)
        {
            var emailTemplate = await LoadEmailTemplate(_config.GetSection("EmailTemplates:VerifyTemplatePath").Value);
            emailTemplate = emailTemplate.Replace("[[FirstName]]", user.FirstName);
            emailTemplate = emailTemplate.Replace("[[verificationLink]]", url);
            emailTemplate = emailTemplate.Replace("[[Email]]", user.Email);
            await SendEmail(user.Email, "Email Verification", emailTemplate);
      
        }

        private async Task<string> LoadEmailTemplate(string templatePath)
        {
            try
            {
                string templateContent = await File.ReadAllTextAsync(templatePath, Encoding.UTF8);
                return templateContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load email template: {ex.Message}");
                throw;
            }
        }
        private async Task SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                
                using (var smtpClient = new SmtpClient(_config.GetSection("EmailSettings:SmtpServer").Value, Convert.ToInt32(_config.GetSection("EmailSettings:SmtpPort").Value)))
                {
                    smtpClient.Credentials = new NetworkCredential(_config.GetSection("EmailSettings:SmtpUsername").Value, _config.GetSection("EmailSettings:SmtpPassword").Value);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_config.GetSection("EmailSettings:SmtpUsername").Value),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                throw;
            }
        }
        private string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 6)
            {
                throw new ArgumentException("Account number must have at least 6 digits.", nameof(accountNumber));
            }

            // Get the first three digits
            string firstThree = accountNumber.Substring(0, 3);

            // Get the last three digits
            string lastThree = accountNumber.Substring(accountNumber.Length - 3);

            // Masked format
            return $"{firstThree}xxx{lastThree}";
        }

    }
}
