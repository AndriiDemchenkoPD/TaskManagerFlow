using TaskManagerApi.Models;

namespace TaskManagerApi.Repositories
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken> AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
        Task<PasswordResetToken?> GetActiveByTokenHashAsync(string tokenHash, DateTime nowUtc, CancellationToken cancellationToken = default);
        Task InvalidateUserTokensAsync(int userId, CancellationToken cancellationToken = default);
        Task UpdateAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    }
}
