using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net.Mail;
using TaskManagerApi.DTOs;
using TaskManagerApi.Services;
using Microsoft.Extensions.Logging;

namespace TaskManagerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AuthService _authService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration config, AuthService authService, IEmailService emailService, ILogger<AuthController> logger)
        {
            _config = config;
            _authService = authService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.RegisterAsync(request, cancellationToken);

            if (!result.Success)
            {
                if (result.ErrorCode == "email_exists")
                {
                    return Conflict(new { message = result.ErrorMessage });
                }

                return BadRequest(new { message = result.ErrorMessage ?? "Registration failed" });
            }

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var authResult = await _authService.LoginAsync(request, cancellationToken);

            if (authResult is null)
            {
                _logger.LogWarning("Invalid login attempt for email: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var token = GenerateJwtToken(authResult.UserId, authResult.Username, authResult.Email);
            return Ok(new LoginResponse { Token = token });
        }

        [HttpPost("forgot-password")]
        [EnableRateLimiting("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var resetBaseUrl = _config["Auth:ResetPasswordUrl"] ?? "http://localhost:5173/reset-password";

            try
            {
                await _authService.RequestPasswordResetAsync(request.Email, resetBaseUrl, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Forgot password email configuration error");
                return StatusCode(500, new { message = "Email service is not configured. Please set SMTP settings." });
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error while sending password reset email");
                return StatusCode(500, new { message = "Failed to send reset email. Please try again later." });
            }

            return Ok(new { message = "If the account exists, a reset link has been sent." });
        }

        [HttpGet("email-status")]
        public IActionResult EmailStatus()
        {
            return Ok(new { configured = _emailService.IsConfigured() });
        }

        [HttpPost("email-test")]
        public async Task<IActionResult> SendEmailTest([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                await _emailService.SendTestEmailAsync(request.Email, cancellationToken);
                return Ok(new { message = "Test email sent successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMTP test email");
                return StatusCode(500, new { message = "SMTP test failed. Check server configuration and credentials." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.ResetPasswordAsync(request, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage ?? "Password reset failed" });
            }

            return Ok(new { message = "Password has been reset successfully" });
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "API is working", timestamp = DateTime.Now });
        }

        private string GenerateJwtToken(int userId, string username, string email)
        {
            var jwt = _config.GetSection("Jwt");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email)
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
