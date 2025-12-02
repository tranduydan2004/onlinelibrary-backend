using System.Net;
using System.Net.Mail;
using OnlineLibrary.Application.Common;

namespace OnlineLibrary.Application.Services
{
    public class StmpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public StmpEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var section = _configuration.GetSection("Smtp");
            var host = section["Host"];
            var port = int.Parse(section["Port"]);
            var user = section["User"];
            var pass = section["Password"];
            var from = section["From"];

            using var message = new MailMessage(from!, toEmail, subject, body)
            {
                IsBodyHtml = false
            };

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}
