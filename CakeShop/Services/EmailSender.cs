using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CakeShop.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var portRaw = _config["EmailSettings:Port"];
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var username = _config["EmailSettings:Username"];
            var password = _config["EmailSettings:Password"];

            if (string.IsNullOrWhiteSpace(smtpServer) ||
                string.IsNullOrWhiteSpace(portRaw) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("EmailSettings 未完整設定，請檢查環境變數或 appsettings。");
            }

            if (!int.TryParse(portRaw, out var port))
            {
                throw new InvalidOperationException("EmailSettings:Port 格式錯誤，必須是數字。");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("蛋糕訂購網", senderEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

            using var client = new SmtpClient
            {
                CheckCertificateRevocation = false
            };

            try
            {
                var socketOptions = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

                await client.ConnectAsync(smtpServer, port, socketOptions);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);

                _logger.LogInformation("驗證信已送出至 {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "寄送驗證信失敗。SMTP={SmtpServer}, Port={Port}, Username={Username}", smtpServer, port, username);
                throw;
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}
