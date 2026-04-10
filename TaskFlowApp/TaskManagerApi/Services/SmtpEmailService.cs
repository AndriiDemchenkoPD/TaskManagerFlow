using System.Net;
using System.Net.Mail;

namespace TaskManagerApi.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
        {
            var smtp = ReadSmtpSettings(throwOnMissing: false);

            if (smtp is null)
            {
                _logger.LogWarning("SMTP not configured. Password reset link for {Email}: {ResetLink}. Expires UTC: {ExpiresAtUtc}", toEmail, resetLink, expiresAtUtc);
                return;
            }

            using var message = new MailMessage
            {
                From = new MailAddress(smtp.From, "TaskFlow Security"),
                Subject = "TaskFlow Password Reset",
                Body = BuildHtmlBody(resetLink, expiresAtUtc),
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(toEmail));

            using var smtpClient = BuildClient(smtp);

            cancellationToken.ThrowIfCancellationRequested();
            await smtpClient.SendMailAsync(message);
            _logger.LogInformation("Password reset email sent to {Email}", toEmail);
        }

        public bool IsConfigured()
        {
            var host = _configuration["Email:Smtp:Host"];
            var from = _configuration["Email:Smtp:From"];
            return !string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(from);
        }

        public async Task SendTestEmailAsync(string toEmail, CancellationToken cancellationToken = default)
        {
            var smtp = ReadSmtpSettings(throwOnMissing: true)!;

            using var message = new MailMessage
            {
                From = new MailAddress(smtp.From, "TaskFlow Security"),
                Subject = "TaskFlow SMTP Test Email",
                Body = "SMTP configuration is valid. This is a test email from TaskFlow.",
                IsBodyHtml = false
            };
            message.To.Add(new MailAddress(toEmail));

            using var smtpClient = BuildClient(smtp);

            cancellationToken.ThrowIfCancellationRequested();
            await smtpClient.SendMailAsync(message);
            _logger.LogInformation("SMTP test email sent to {Email}", toEmail);
        }

        private SmtpSettings? ReadSmtpSettings(bool throwOnMissing)
        {
            var settings = new SmtpSettings
            {
                Host = _configuration["Email:Smtp:Host"] ?? string.Empty,
                From = _configuration["Email:Smtp:From"] ?? string.Empty,
                Username = _configuration["Email:Smtp:Username"],
                Password = _configuration["Email:Smtp:Password"],
                EnableSsl = bool.TryParse(_configuration["Email:Smtp:EnableSsl"], out var ssl) && ssl,
                Port = int.TryParse(_configuration["Email:Smtp:Port"], out var parsedPort) ? parsedPort : 587
            };

            if (string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.From))
            {
                if (throwOnMissing)
                {
                    throw new InvalidOperationException("SMTP is not configured. Set Email:Smtp:Host and Email:Smtp:From.");
                }

                return null;
            }

            return settings;
        }

        private static SmtpClient BuildClient(SmtpSettings smtp)
        {
            var smtpClient = new SmtpClient(smtp.Host, smtp.Port)
            {
                EnableSsl = smtp.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(smtp.Username) && !string.IsNullOrWhiteSpace(smtp.Password))
            {
                smtpClient.Credentials = new NetworkCredential(smtp.Username, smtp.Password);
            }

            return smtpClient;
        }

        private static string BuildHtmlBody(string resetLink, DateTime expiresAtUtc)
        {
            return $@"
<html>
  <body style='font-family:Segoe UI,Arial,sans-serif;line-height:1.5;color:#111827;'>
    <h2>Reset your password</h2>
    <p>We received a request to reset your TaskFlow password.</p>
    <p>
      <a href='{resetLink}' style='background:#2563eb;color:#fff;padding:10px 16px;border-radius:8px;text-decoration:none;'>Reset Password</a>
    </p>
    <p>Or use this link directly:</p>
    <p><a href='{resetLink}'>{resetLink}</a></p>
    <p>This link expires at <strong>{expiresAtUtc:yyyy-MM-dd HH:mm:ss} UTC</strong> and can be used only once.</p>
    <p>If you did not request this, you can ignore this email.</p>
  </body>
</html>";
        }

        private sealed class SmtpSettings
        {
            public required string Host { get; set; }
            public required string From { get; set; }
            public string? Username { get; set; }
            public string? Password { get; set; }
            public int Port { get; set; }
            public bool EnableSsl { get; set; }
        }
    }
}
