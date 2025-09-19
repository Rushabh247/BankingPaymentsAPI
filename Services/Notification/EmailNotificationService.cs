using System.Net;
using System.Net.Mail;

namespace BankingPaymentsAPI.Services.Notification
{
   

    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IConfiguration _config;

        public EmailNotificationService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = int.Parse(_config["Smtp:Port"]);
            var smtpUser = _config["Smtp:Username"];
            var smtpPass = _config["Smtp:Password"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mail = new MailMessage(smtpUser, to, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail);
        }
    }

}
