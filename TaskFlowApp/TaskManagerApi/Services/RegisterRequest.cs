namespace TaskManagerApi.DTOs
{
    public class RegisterRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(20, MinimumLength = 3)]
        [System.ComponentModel.DataAnnotations.RegularExpression("^[A-Za-z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
        public required string Username { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public required string Email { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(8)]
        [System.ComponentModel.DataAnnotations.RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z\\d]).{8,}$", ErrorMessage = "Password must include uppercase, lowercase, number, and special character")]
        public required string Password { get; set; }
    }
}
