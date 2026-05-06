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
        private readonly IWebHostEnvironment _env;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger, IWebHostEnvironment env)
        {
            _config = config;
            _logger = logger;
            _env = env;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var portRaw = _config["EmailSettings:Port"];
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var username = _config["EmailSettings:Username"];
            var password = _config["EmailSettings:Password"];
            var throwOnFailure = false;
            var timeoutMsRaw = _config["EmailSettings:TimeoutMs"];
            var timeoutMs = 5000;

            if (string.IsNullOrWhiteSpace(smtpServer) ||
                string.IsNullOrWhiteSpace(portRaw) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                var ex = new InvalidOperationException("EmailSettings 未完整設定，請檢查環境變數或 appsettings。");
                _logger.LogError(ex, "寄送驗證信前檢查失敗。ThrowOnFailure={ThrowOnFailure}", throwOnFailure);

                _logger.LogWarning("已略過寄信，註冊流程繼續。請確認 Render 已設定 EmailSettings__* 並重新部署。");
                return;
            }

            if (!int.TryParse(portRaw, out var port))
            {
                var ex = new InvalidOperationException("EmailSettings:Port 格式錯誤，必須是數字。");
                _logger.LogError(ex, "寄送驗證信前檢查失敗。ThrowOnFailure={ThrowOnFailure}", throwOnFailure);

                _logger.LogWarning("已略過寄信，註冊流程繼續。請修正 EmailSettings__Port。");
                return;
            }

            if (!string.IsNullOrWhiteSpace(timeoutMsRaw) && (!int.TryParse(timeoutMsRaw, out timeoutMs) || timeoutMs <= 0))
            {
                _logger.LogWarning("EmailSettings:TimeoutMs 設定無效，將使用預設值 5000ms。Raw={TimeoutMsRaw}", timeoutMsRaw);
                timeoutMs = 5000;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("蛋糕訂購網", senderEmail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

            using var client = new SmtpClient
            {
                CheckCertificateRevocation = false,
                Timeout = timeoutMs
            };

            try
            {
                var socketOptions = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

                using var cts = new CancellationTokenSource(timeoutMs);

                await client.ConnectAsync(smtpServer, port, socketOptions, cts.Token);
                await client.AuthenticateAsync(username, password, cts.Token);
                await client.SendAsync(message, cts.Token);

                _logger.LogInformation("驗證信已送出至 {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "寄送驗證信失敗。SMTP={SmtpServer}, Port={Port}, Username={Username}, ThrowOnFailure={ThrowOnFailure}", smtpServer, port, username, throwOnFailure);

                _logger.LogWarning("驗證信寄送失敗已略過，註冊流程將繼續。請檢查 Render 環境變數 EmailSettings__* 是否正確。");
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
