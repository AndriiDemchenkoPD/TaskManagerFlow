using TaskManagerApi.Models;

namespace TaskManagerApi.Repositories
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
        Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<AppUser> AddAsync(AppUser user, CancellationToken cancellationToken = default);
        Task UpdateAsync(AppUser user, CancellationToken cancellationToken = default);
    }
}
