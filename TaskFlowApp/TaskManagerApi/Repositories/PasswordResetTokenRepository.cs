using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.Models;

namespace TaskManagerApi.Repositories
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly TaskManagerDbContext _dbContext;

        public PasswordResetTokenRepository(TaskManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PasswordResetToken> AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        {
            _dbContext.PasswordResetTokens.Add(token);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return token;
        }

        public Task<PasswordResetToken?> GetActiveByTokenHashAsync(string tokenHash, DateTime nowUtc, CancellationToken cancellationToken = default)
        {
            return _dbContext.PasswordResetTokens
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash && x.UsedAtUtc == null && x.ExpiresAtUtc > nowUtc, cancellationToken);
        }

        public async Task InvalidateUserTokensAsync(int userId, CancellationToken cancellationToken = default)
        {
            var activeTokens = await _dbContext.PasswordResetTokens
                .Where(x => x.AppUserId == userId && x.UsedAtUtc == null)
                .ToListAsync(cancellationToken);

            if (activeTokens.Count == 0)
            {
                return;
            }

            var nowUtc = DateTime.UtcNow;
            foreach (var token in activeTokens)
            {
                token.UsedAtUtc = nowUtc;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        {
            _dbContext.PasswordResetTokens.Update(token);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
