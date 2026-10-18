using FitZone.Application.ServiceInterfaces;
using Microsoft.Extensions.Logging;


namespace FitZone.Infrastructure.Email
{
    public class DevelopmentEmailSender : IEmailSender
    {
        private readonly ILogger<DevelopmentEmailSender> _logger;

        public DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
        {
            var text = System.Text.RegularExpressions.Regex.Replace(body, "<.*?>", " ");
            _logger.LogInformation("FAKE EMAIL to {To}: {Subject}\n{Body}", toEmail, subject, body, ct);
            return Task.CompletedTask;
        }
    }



}
