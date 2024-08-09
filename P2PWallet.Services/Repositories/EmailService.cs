using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using P2PWallet.Models.DTOs;
using P2PWallet.Models.Entities;
using P2PWallet.Services.Interfaces;

namespace P2PWallet.Services.Repositories
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _smtpServer = config["EmailSettings:SmtpServer"];
            _smtpPort = int.Parse(config["EmailSettings:SmtpPort"]);
            _smtpUsername = config["EmailSettings:SmtpUsername"];
            _smtpPassword = config["EmailSettings:SmtpPassword"];
            _logger = logger;
        }

        public async Task SendCreditEmail(EmailDTO emailDTO)
        {
            var emailTemplate = await LoadEmailTemplate("EmailTemplates:CreditTemplatePath");
            emailTemplate = ReplaceTemplateVariables(emailTemplate, emailDTO);
            await SendEmail(emailDTO.Email, "Credit Notification", emailTemplate);
        }

        public async Task SendDebitEmail(EmailDTO emailDTO)
        {
            var emailTemplate = await LoadEmailTemplate("EmailTemplates:DebitTemplatePath");
            emailTemplate = ReplaceTemplateVariables(emailTemplate, emailDTO);
            await SendEmail(emailDTO.Email, "Debit Notification", emailTemplate);
        }

        public async Task SendResetTokenEmail(User user, string url)
        {
            var emailTemplate = await LoadEmailTemplate("EmailTemplates:ResetTokenTemplatePath");
            emailTemplate = emailTemplate.Replace("[[FirstName]]", user.FirstName)
                                         .Replace("[[ResetLink]]", url);
            await SendEmail(user.Email, "Password Reset", emailTemplate);
        }

        public async Task SendVerificationEmail(User user, string url)
        {
            var emailTemplate = await LoadEmailTemplate("EmailTemplates:VerifyTemplatePath");
            emailTemplate = emailTemplate.Replace("[[FirstName]]", user.FirstName)
                                         .Replace("[[verificationLink]]", url)
                                         .Replace("[[Email]]", user.Email);
            await SendEmail(user.Email, "Email Verification", emailTemplate);
        }

        private async Task<string> LoadEmailTemplate(string templateConfigKey)
        {
            var templatePath = _config.GetSection(templateConfigKey).Value;
            if (string.IsNullOrEmpty(templatePath))
            {
                throw new InvalidOperationException($"Email template path not found for key: {templateConfigKey}");
            }

            try
            {
                return await File.ReadAllTextAsync(templatePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to load email template from path: {templatePath}");
                throw;
            }
        }

        private string ReplaceTemplateVariables(string template, EmailDTO emailDTO)
        {
            return template.Replace("[[Amount]]", emailDTO.Amount.ToString())
                           .Replace("[[Balance]]", emailDTO.Balance.ToString())
                           .Replace("[[Date]]", emailDTO.TransactionDate.ToString())
                           .Replace("[[TransactionID]]", emailDTO.TransactionID.ToString())
                           .Replace("[[AccountNumber]]", MaskAccountNumber(emailDTO.AccountNumber.ToString()))
                           .Replace("[[Currency]]", emailDTO.Currency.ToString())
                           .Replace("[[FirstName]]", emailDTO.FirstName)
                           .Replace("[[LastName]]", emailDTO.LastName);
        }

        private async Task SendEmail(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_smtpUsername));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_smtpUsername, _smtpPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                throw;
            }
        }

        private string MaskAccountNumber(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length < 6)
            {
                throw new ArgumentException("Account number must have at least 6 digits.", nameof(accountNumber));
            }

            return $"{accountNumber.Substring(0, 3)}xxx{accountNumber.Substring(accountNumber.Length - 3)}";
        }
    }
}
