using System.ComponentModel.DataAnnotations;

namespace TaskManagerApi.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
