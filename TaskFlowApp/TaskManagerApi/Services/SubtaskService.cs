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
            EnsureSubtaskTableSchema();
        }

        private void EnsureSubtaskTableSchema()
        {
            using var conn = new SqlConnection(Conn);
            const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Subtasks' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Subtasks (
        SubtaskId INT PRIMARY KEY IDENTITY(1,1),
        ParentTaskId INT NOT NULL,
        Title NVARCHAR(255) NOT NULL,
        IsCompleted BIT NOT NULL CONSTRAINT DF_Subtasks_IsCompleted DEFAULT 0,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Subtasks_CreatedAt DEFAULT GETDATE(),
        CompletedAt DATETIME NULL,
        FOREIGN KEY (ParentTaskId) REFERENCES dbo.Tasks(TaskId) ON DELETE CASCADE
    );
END

IF COL_LENGTH('dbo.Subtasks', 'IsCompleted') IS NULL
    ALTER TABLE dbo.Subtasks ADD IsCompleted BIT NOT NULL CONSTRAINT DF_Subtasks_IsCompleted_Alt DEFAULT 0;

IF COL_LENGTH('dbo.Subtasks', 'CreatedAt') IS NULL
    ALTER TABLE dbo.Subtasks ADD CreatedAt DATETIME NOT NULL CONSTRAINT DF_Subtasks_CreatedAt_Alt DEFAULT GETDATE();

IF COL_LENGTH('dbo.Subtasks', 'CompletedAt') IS NULL
    ALTER TABLE dbo.Subtasks ADD CompletedAt DATETIME NULL;
";

            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public List<Subtask> GetSubtasks(int parentTaskId, int userId)
        {
            var subtasks = new List<Subtask>();
            using var conn = new SqlConnection(Conn);
            string query = @"SELECT s.*
                             FROM Subtasks s
                             INNER JOIN Tasks t ON t.TaskId = s.ParentTaskId
                             WHERE s.ParentTaskId=@pid AND t.UserId=@uid
                             ORDER BY s.CreatedAt";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", parentTaskId);
            cmd.Parameters.AddWithValue("@uid", userId);
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

        public bool CreateSubtask(Subtask subtask, int userId)
        {
            using var conn = new SqlConnection(Conn);
            string query = @"
                IF EXISTS (SELECT 1 FROM Tasks WHERE TaskId=@pid AND UserId=@uid)
                    INSERT INTO Subtasks (ParentTaskId, Title, CreatedAt) VALUES (@pid, @title, @createdAt)";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", subtask.ParentTaskId);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@title", subtask.Title);
            cmd.Parameters.AddWithValue("@createdAt", subtask.CreatedAt == default ? DateTime.Now : subtask.CreatedAt);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateSubtaskStatus(int subtaskId, bool isCompleted, int userId)
        {
            using var conn = new SqlConnection(Conn);
            string query = @"UPDATE s
                           SET s.IsCompleted=@status, s.CompletedAt=@time
                           FROM Subtasks s
                           INNER JOIN Tasks t ON t.TaskId = s.ParentTaskId
                           WHERE s.SubtaskId=@id AND t.UserId=@uid";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@status", isCompleted);
            cmd.Parameters.AddWithValue("@time", isCompleted ? DateTime.Now : DBNull.Value);
            cmd.Parameters.AddWithValue("@id", subtaskId);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteSubtask(int subtaskId, int userId)
        {
            using var conn = new SqlConnection(Conn);
            string query = @"DELETE s
                           FROM Subtasks s
                           INNER JOIN Tasks t ON t.TaskId = s.ParentTaskId
                           WHERE s.SubtaskId=@id AND t.UserId=@uid";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", subtaskId);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public double GetSubtaskCompletionPercentage(int parentTaskId, int userId)
        {
            using var conn = new SqlConnection(Conn);
            string query = @"SELECT 
                           CASE WHEN COUNT(*) = 0 THEN 0 
                           ELSE CAST(SUM(CASE WHEN IsCompleted=1 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100 
                           END 
                           FROM Subtasks s
                           INNER JOIN Tasks t ON t.TaskId = s.ParentTaskId
                           WHERE s.ParentTaskId=@pid AND t.UserId=@uid";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pid", parentTaskId);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            return (double)(cmd.ExecuteScalar() ?? 0);
        }
    }
}
