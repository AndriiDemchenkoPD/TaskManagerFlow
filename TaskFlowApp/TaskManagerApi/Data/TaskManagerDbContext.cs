using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Models;

namespace TaskManagerApi.Data
{
    public class TaskManagerDbContext : DbContext
    {
        public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options) : base(options)
        {
        }

        public DbSet<AppUser> AppUsers => Set<AppUser>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUsers");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Username)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.PasswordHash)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.HasIndex(x => x.Username).IsUnique();
                entity.HasIndex(x => x.Email).IsUnique();
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.ToTable("PasswordResetTokens");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.TokenHash)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.CreatedAtUtc).IsRequired();
                entity.Property(x => x.ExpiresAtUtc).IsRequired();

                entity.HasIndex(x => x.TokenHash).IsUnique();
                entity.HasIndex(x => new { x.AppUserId, x.ExpiresAtUtc });

                entity.HasOne(x => x.AppUser)
                    .WithMany(x => x.PasswordResetTokens)
                    .HasForeignKey(x => x.AppUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
