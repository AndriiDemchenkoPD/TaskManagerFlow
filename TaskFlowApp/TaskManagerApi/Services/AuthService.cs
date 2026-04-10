using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using TaskManagerApi.DTOs;
using TaskManagerApi.Models;
using TaskManagerApi.Repositories;

namespace TaskManagerApi.Services
{
    public class AuthService
    {
        private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromMinutes(15);

        private readonly ILogger<AuthService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IEmailService _emailService;

        public AuthService(
            ILogger<AuthService> logger,
            IUserRepository userRepository,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IEmailService emailService)
        {
            _logger = logger;
            _userRepository = userRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _emailService = emailService;
        }

        public async Task<(bool Success, string? ErrorCode, string? ErrorMessage)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            var normalizedUsername = request.Username.Trim();
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            if (await _userRepository.UsernameExistsAsync(normalizedUsername, cancellationToken))
            {
                return (false, "username_exists", "Username already exists");
            }

            if (await _userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
            {
                return (false, "email_exists", "Email already exists");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new AppUser
            {
                Username = normalizedUsername,
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.AddAsync(user, cancellationToken);
            _logger.LogInformation("User registered successfully: {Username}", normalizedUsername);
            return (true, null, null);
        }

        public async Task<AuthResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

            if (user is null)
            {
                return null;
            }

            var isPasswordValid = VerifyBcryptSafe(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                var legacyHash = ComputeLegacySha256Hash(request.Password);
                if (!string.Equals(legacyHash, user.PasswordHash, StringComparison.Ordinal))
                {
                    return null;
                }

                // Upgrade legacy SHA256 hash to BCrypt on successful login.
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                await _userRepository.UpdateAsync(user, cancellationToken);
                _logger.LogInformation("Upgraded legacy password hash for user {UserId}", user.Id);
            }

            return new AuthResult
            {
                UserId = user.Id,
                Email = user.Email,
                Username = user.Username,
                Token = string.Empty
            };
        }

        public async Task RequestPasswordResetAsync(string email, string resetBaseUrl, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

            if (user is null)
            {
                _logger.LogInformation("Password reset requested for unknown email");
                return;
            }

            await _passwordResetTokenRepository.InvalidateUserTokensAsync(user.Id, cancellationToken);

            var rawToken = GenerateSecureToken();
            var tokenHash = HashToken(rawToken);
            var nowUtc = DateTime.UtcNow;

            var token = new PasswordResetToken
            {
                AppUserId = user.Id,
                TokenHash = tokenHash,
                CreatedAtUtc = nowUtc,
                ExpiresAtUtc = nowUtc.Add(ResetTokenLifetime)
            };

            await _passwordResetTokenRepository.AddAsync(token, cancellationToken);

            var resetLink = BuildResetLink(resetBaseUrl, rawToken);
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink, token.ExpiresAtUtc, cancellationToken);
        }

        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
            {
                return (false, "Password confirmation does not match");
            }

            var tokenHash = HashToken(request.Token);
            var nowUtc = DateTime.UtcNow;
            var token = await _passwordResetTokenRepository.GetActiveByTokenHashAsync(tokenHash, nowUtc, cancellationToken);

            if (token is null)
            {
                return (false, "Reset token is invalid or expired");
            }

            var user = await _userRepository.GetByIdAsync(token.AppUserId, cancellationToken);

            if (user is null)
            {
                return (false, "Reset token is invalid or expired");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepository.UpdateAsync(user, cancellationToken);

            token.UsedAtUtc = nowUtc;
            await _passwordResetTokenRepository.UpdateAsync(token, cancellationToken);

            return (true, null);
        }

        public async Task<int> GetUserIdAsync(string username, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
            return user?.Id ?? throw new Exception("User not found");
        }

        public int GetUserId(string username)
        {
            return GetUserIdAsync(username).GetAwaiter().GetResult();
        }

        private static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        private static string HashToken(string rawToken)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(rawToken);
            return Convert.ToHexString(sha.ComputeHash(bytes));
        }

        private static string BuildResetLink(string resetBaseUrl, string rawToken)
        {
            var separator = resetBaseUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";
            return $"{resetBaseUrl}{separator}token={Uri.EscapeDataString(rawToken)}";
        }

        private static bool VerifyBcryptSafe(string rawPassword, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash) || !passwordHash.StartsWith("$2", StringComparison.Ordinal))
            {
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(rawPassword, passwordHash);
            }
            catch
            {
                return false;
            }
        }

        private static string ComputeLegacySha256Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
