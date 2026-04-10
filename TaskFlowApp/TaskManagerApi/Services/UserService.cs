using System.Data;
using Microsoft.Data.SqlClient;
using TaskManagerApi.DTOs;
using TaskManagerApi.Models;

namespace TaskManagerApi.Services
{
    public class UserService
    {
        private readonly IConfiguration _config;

        public UserService(IConfiguration config)
        {
            _config = config;
        }

        public bool ValidateUser(LoginRequest request)
        {
            var connectionString =
                _config.GetConnectionString("BTConnection");

            using SqlConnection conn = new SqlConnection(connectionString);

            string query = @"
                SELECT COUNT(1)
                FROM AppUsers
                WHERE Email = @email";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", request.Email);

            conn.Open();

            int count = (int)cmd.ExecuteScalar();

            return count > 0;
        }

        public AppUser? GetUserById(int userId)
        {
            var connectionString = _config.GetConnectionString("BTConnection");
            using SqlConnection conn = new SqlConnection(connectionString);

            string query = "SELECT Id, Username, Email, PasswordHash FROM AppUsers WHERE Id = @userId";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            conn.Open();
            var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new AppUser
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    PasswordHash = reader.GetString(3)
                };
            }

            return null;
        }

        public bool UpdateUserProfile(int userId, string username, string email)
        {
            var connectionString = _config.GetConnectionString("BTConnection");
            using SqlConnection conn = new SqlConnection(connectionString);

            string query = @"UPDATE AppUsers 
                           SET Username = @username, Email = @email 
                           WHERE Id = @userId";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@userId", userId);

            conn.Open();
            int rowsAffected = cmd.ExecuteNonQuery();

            return rowsAffected > 0;
        }
    }
}
