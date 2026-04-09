using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration config, AuthService authService, ILogger<AuthController> logger)
        {
            _config = config;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
            _logger.LogInformation($"Registration attempt for username: {request.Username}, email: {request.Email}");
            try
            {
                var success = _authService.Register(
                    request.Username,
                    request.Email,
                    request.Password);

                if (!success)
                    return BadRequest(new { message = "Username or email already exists, or registration failed" });

                return Ok(new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error for {request.Username}: {ex.Message}");
                return BadRequest(new { message = $"Registration error: {ex.Message}" });
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Login attempt with missing username or password");
                return BadRequest(new { message = "Username and password are required" });
            }

            try
            {
                bool valid = _authService.ValidateUser(request.Username, request.Password);

                if (!valid)
                {
                    _logger.LogWarning("Invalid login attempt for username: {Username}", request.Username);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                var token = GenerateJwtToken(request.Username);
                return Ok(new LoginResponse { Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
                return StatusCode(500, new { message = "Login failed due to server error" });
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "API is working", timestamp = DateTime.Now });
        }

        private string GenerateJwtToken(string username)
        {
            var jwt = _config.GetSection("Jwt");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username)
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
