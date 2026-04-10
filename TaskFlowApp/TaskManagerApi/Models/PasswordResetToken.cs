namespace TaskManagerApi.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int AppUserId { get; set; }
        public required string TokenHash { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UsedAtUtc { get; set; }

        public AppUser? AppUser { get; set; }

        public bool IsExpired(DateTime nowUtc) => ExpiresAtUtc <= nowUtc;
        public bool IsUsed => UsedAtUtc.HasValue;
    }
}
