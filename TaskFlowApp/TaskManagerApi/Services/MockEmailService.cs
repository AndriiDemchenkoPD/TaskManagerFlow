using Microsoft.Extensions.Logging;

namespace TaskManagerApi.Services
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
        }

        public Task SendPasswordResetEmailAsync(string toEmail, string resetLink, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Password reset email (mock) to {Email}. Expires UTC: {ExpiresAtUtc}. Reset link: {ResetLink}", toEmail, expiresAtUtc, resetLink);
            return Task.CompletedTask;
        }

        public bool IsConfigured()
        {
            return true;
        }

        public Task SendTestEmailAsync(string toEmail, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("SMTP test email (mock) to {Email}", toEmail);
            return Task.CompletedTask;
        }
    }
}
