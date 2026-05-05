using MailKit.Net.Smtp;
using MailKit.Security;
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
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                "蛋糕訂購網",
                _config["EmailSettings:SenderEmail"]
            ));

            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            message.Body = new BodyBuilder
            {
                HtmlBody = htmlMessage
            }.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                var host = _config["EmailSettings:SmtpServer"];
                var port = _config.GetValue<int>("EmailSettings:Port");
                var username = _config["EmailSettings:Username"];
                var password = _config["EmailSettings:Password"];

                // ⭐ 正確 SSL 設定（比 true/false 安全）
                var secureSocketOptions =
                    port == 465
                        ? SecureSocketOptions.SslOnConnect
                        : SecureSocketOptions.StartTls;

                await client.ConnectAsync(host, port, secureSocketOptions);

                await client.AuthenticateAsync(username, password);

                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                // 建議你換成 ILogger（這裡先保守寫）
                Console.WriteLine($"Email 發送失敗: {ex.Message}");
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