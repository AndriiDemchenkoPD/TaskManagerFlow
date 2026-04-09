using Microsoft.Data.SqlClient;
using TaskManagerApi.Models;

namespace TaskManagerApi.Services
{
    public class CommentService
    {
        private readonly IConfiguration _config;
        private string Conn => _config.GetConnectionString("BTConnection")!;

        public CommentService(IConfiguration config)
        {
            _config = config;
        }

        public List<TaskComment> GetTaskComments(int taskId)
        {
            var comments = new List<TaskComment>();
            using var conn = new SqlConnection(Conn);
            string query = @"SELECT c.*, u.Username FROM TaskComments c 
                           LEFT JOIN AppUsers u ON c.UserId = u.Id 
                           WHERE c.TaskId=@tid AND c.IsDeleted=0 
                           ORDER BY c.CreatedAt DESC";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@tid", taskId);
            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                comments.Add(new TaskComment
                {
                    CommentId = reader.GetInt32(0),
                    TaskId = reader.GetInt32(1),
                    Id = reader.GetInt32(2),
                    CommentText = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    UpdatedAt = reader.GetDateTime(5),
                    IsDeleted = reader.GetBoolean(6),
                    UserName = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }
            return comments;
        }

        public bool AddComment(TaskComment comment)
        {
            using var conn = new SqlConnection(Conn);
            string query = @"INSERT INTO TaskComments (TaskId, UserId, CommentText) 
                           VALUES (@tid, @uid, @text)";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@tid", comment.TaskId);
            cmd.Parameters.AddWithValue("@uid", comment.Id);
            cmd.Parameters.AddWithValue("@text", comment.CommentText);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateComment(int commentId, string text, int userId)
        {
            using var conn = new SqlConnection(Conn);
            string query = @"UPDATE TaskComments SET CommentText=@text, UpdatedAt=GETDATE() 
                           WHERE CommentId=@id AND UserId=@uid";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@text", text);
            cmd.Parameters.AddWithValue("@id", commentId);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteComment(int commentId, int userId)
        {
            using var conn = new SqlConnection(Conn);
            string query = "UPDATE TaskComments SET IsDeleted=1 WHERE CommentId=@id AND UserId=@uid";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", commentId);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
