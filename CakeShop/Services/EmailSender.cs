using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CakeShop.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailMessage = new MimeMessage();
            // 請替換為你的寄件者信箱與名稱
            emailMessage.From.Add(new MailboxAddress("蛋糕訂購網", _config["EmailSettings:SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // 這裡以 Gmail SMTP 為例，請在 appsettings.json 中配置
                await client.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:Port"]), true);
                await client.AuthenticateAsync(_config["EmailSettings:Username"], _config["EmailSettings:Password"]);
                await client.SendAsync(emailMessage);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}