using System.ComponentModel.DataAnnotations;

namespace TaskManagerApi.DTOs
{
    public class ResetPasswordRequest
    {
        [Required]
        public required string Token { get; set; }

        [Required]
        [MinLength(8)]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z\\d]).{8,}$", ErrorMessage = "Password must include uppercase, lowercase, number, and special character")]
        public required string NewPassword { get; set; }

        [Required]
        public required string ConfirmPassword { get; set; }
    }
}
