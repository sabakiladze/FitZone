using FitZone.Application.ServiceInterfaces;
using FitZone.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Infrastructure.Email
{
    public class SmtpEmailSenderService : IEmailSender

    {

        private readonly EmailSettings _emailSettings;
        public SmtpEmailSenderService(IOptions<EmailSettings> options)
        {
            _emailSettings = options.Value;
        }
        public async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
        {
            using SmtpClient smtp = new(_emailSettings.SmtpServer)
            {
                Port = _emailSettings.Port,
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _emailSettings.SenderEmail,
                    _emailSettings.AppPassword)
            };

            using MailMessage message = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);
           await smtp.SendMailAsync(message, ct);

        }
    }
}
