using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TaskManagerApi.Services
{
    public class AuthService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration config, ILogger<AuthService> logger)
        {
            _config = config;
            _logger = logger;
        }

        private string HashPassword(string password)
        {
            using SHA256 sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool Register(string username, string email, string password)
        {
            _logger.LogDebug($"Registration attempt for username: {username}");
            var connStr = _config.GetConnectionString("BTConnection");

            try
            {
                using SqlConnection conn = new SqlConnection(connStr);

                string query = @"INSERT INTO AppUsers
                                 (Username, Email, PasswordHash)
                                 VALUES (@u,@e,@p)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@u", username ?? "");
                cmd.Parameters.AddWithValue("@e", email ?? "");
                cmd.Parameters.AddWithValue("@p", HashPassword(password));

                conn.Open();
                var result = cmd.ExecuteNonQuery() > 0;
                _logger.LogDebug($"Registration {(result ? "successful" : "failed")} for: {username}");
                return result;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Error during registration: {sqlEx.Message}");
                throw new Exception($"Database error: {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during registration: {ex.Message}");
                throw;
            }
        }

        public bool ValidateUser(string username, string password)
        {
            var login = username?.Trim();

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Login attempt with empty username or password");
                return false;
            }

            _logger.LogDebug("Login attempt for identity: {Login}", login);
            var connStr = _config.GetConnectionString("BTConnection");

            try
            {
                using SqlConnection conn = new SqlConnection(connStr);

                string query = @"SELECT TOP 1 PasswordHash
                             FROM AppUsers
                             WHERE Username=@u OR Email=@u";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", login);

                conn.Open();

                var result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    _logger.LogWarning("Login failed - user not found: {Login}", login);
                    return false;
                }

                string storedHash = result.ToString() ?? string.Empty;
                string inputHash = HashPassword(password);

                var isValid = storedHash == inputHash;
                _logger.LogDebug("Login {Status} for: {Login}", isValid ? "successful" : "failed", login);
                return isValid;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL exception during ValidateUser for identity: {Login}", login);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during ValidateUser for identity: {Login}", login);
                return false;
            }
        }
        public int GetUserId(string username)
        {
            var connStr = _config.GetConnectionString("BTConnection");

            using SqlConnection conn = new SqlConnection(connStr);

            string query = "SELECT Id FROM AppUsers WHERE Username=@u";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", username);

            conn.Open();

            var result = cmd.ExecuteScalar();

            return result is null 
                ? throw new Exception("User not found")
                : Convert.ToInt32(result);
        }

    }
}
