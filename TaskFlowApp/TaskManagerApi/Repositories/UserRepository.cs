using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.Models;

namespace TaskManagerApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly TaskManagerDbContext _dbContext;

        public UserRepository(TaskManagerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<AppUser?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return _dbContext.AppUsers.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        }

        public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
        {
            return _dbContext.AppUsers.AnyAsync(x => x.Username == username, cancellationToken);
        }

        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return _dbContext.AppUsers.AnyAsync(x => x.Email == email, cancellationToken);
        }

        public Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return _dbContext.AppUsers.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        }

        public Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return _dbContext.AppUsers.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
        }

        public async Task<AppUser> AddAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            _dbContext.AppUsers.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }

        public async Task UpdateAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            _dbContext.AppUsers.Update(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
