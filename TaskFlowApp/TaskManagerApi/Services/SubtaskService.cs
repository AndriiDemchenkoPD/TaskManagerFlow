using Microsoft.Data.SqlClient;
using TaskManagerApi.Models;

namespace TaskManagerApi.Services
{
    public class SubtaskService
    {
        private readonly IConfiguration _config;
        private string Conn => _config.GetConnectionString("BTConnection")!;

        public SubtaskService(IConfiguration config)
        {
            _config = config;
        }

        public List<Subtask> GetSubtasks(int parentTaskId)
        {
            var subtasks = new List<Subtask>();
            using var conn = new SqlConnection(Conn);
            string query = "SELECT * FROM Subtasks WHERE ParentTaskId=@pid ORDER BY CreatedAt";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", parentTaskId);
            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                subtasks.Add(new Subtask
                {
                    SubtaskId = reader.GetInt32(0),
                    ParentTaskId = reader.GetInt32(1),
                    Title = reader.GetString(2),
                    IsCompleted = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4),
                    CompletedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
                });
            }
            return subtasks;
        }

        public bool CreateSubtask(Subtask subtask)
        {
            using var conn = new SqlConnection(Conn);
            string query = "INSERT INTO Subtasks (ParentTaskId, Title) VALUES (@pid, @title)";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", subtask.ParentTaskId);
            cmd.Parameters.AddWithValue("@title", subtask.Title);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateSubtaskStatus(int subtaskId, bool isCompleted)
        {
            using var conn = new SqlConnection(Conn);
            string query = @"UPDATE Subtasks SET IsCompleted=@status, CompletedAt=@time 
                           WHERE SubtaskId=@id";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@status", isCompleted);
            cmd.Parameters.AddWithValue("@time", isCompleted ? DateTime.Now : (object?)null ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", subtaskId);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteSubtask(int subtaskId)
        {
            using var conn = new SqlConnection(Conn);
            string query = "DELETE FROM Subtasks WHERE SubtaskId=@id";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", subtaskId);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public double GetSubtaskCompletionPercentage(int parentTaskId)
        {
            using var conn = new SqlConnection(Conn);
            string query = @"SELECT 
                           CASE WHEN COUNT(*) = 0 THEN 0 
                           ELSE CAST(SUM(CASE WHEN IsCompleted=1 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100 
                           END 
                           FROM Subtasks WHERE ParentTaskId=@pid";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", parentTaskId);
            conn.Open();
            return (double)(cmd.ExecuteScalar() ?? 0);
        }
    }
}
