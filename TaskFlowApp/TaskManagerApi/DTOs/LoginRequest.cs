namespace TaskManagerApi.DTOs
{
    public class LoginRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public required string Email { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public required string Password { get; set; }
    }
}
