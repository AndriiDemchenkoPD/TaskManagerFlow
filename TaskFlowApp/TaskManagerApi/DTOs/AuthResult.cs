namespace TaskManagerApi.DTOs
{
    public class AuthResult
    {
        public required string Token { get; set; }
        public required int UserId { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
    }
}
