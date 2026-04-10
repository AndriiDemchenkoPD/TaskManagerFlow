namespace TaskManagerApi.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink, DateTime expiresAtUtc, CancellationToken cancellationToken = default);
        bool IsConfigured();
        Task SendTestEmailAsync(string toEmail, CancellationToken cancellationToken = default);
    }
}
