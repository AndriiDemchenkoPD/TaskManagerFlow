using Microsoft.Data.SqlClient;
using TaskManagerApi.Models;

namespace TaskManagerApi.Services
{
    public class DashboardService
    {
        private readonly IConfiguration _config;
        private string Conn => _config.GetConnectionString("BTConnection")!;

        public DashboardService(IConfiguration config)
        {
            _config = config;
        }

        public DashboardStats GetDashboardStats(int userId)
        {
            using var conn = new SqlConnection(Conn);
            string query = @"
                SELECT 
                    COUNT(*) as TotalTasks,
                    SUM(CASE WHEN Status='Completed' AND CAST(CompletedAt AS DATE)=CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) as CompletedToday,
                    SUM(CASE WHEN Status!='Completed' AND DueDate < GETDATE() THEN 1 ELSE 0 END) as OverdueTasks,
                    SUM(CASE WHEN Status='In Progress' THEN 1 ELSE 0 END) as InProgressTasks,
                    CAST(SUM(CASE WHEN Status='Completed' THEN 1 ELSE 0 END) AS FLOAT) / NULLIF(COUNT(*), 0) * 100 as CompletionPercentage
                FROM BT_Tasks 
                WHERE UserId=@uid AND IsDeleted=0";
            
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            var reader = cmd.ExecuteReader();
            
            var stats = new DashboardStats();
            if (reader.Read())
            {
                stats.TotalTasks = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                stats.CompletedToday = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                stats.OverdueTasks = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                stats.InProgressTasks = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                stats.CompletionPercentage = reader.IsDBNull(4) ? 0 : reader.GetDouble(4);
            }
            reader.Close();

            // Get total projects
            string projectQuery = "SELECT COUNT(*) FROM Projects WHERE UserId=@uid AND IsDeleted=0";
            var projectCmd = new SqlCommand(projectQuery, conn);
            projectCmd.Parameters.AddWithValue("@uid", userId);
            stats.TotalProjects = (int)(projectCmd.ExecuteScalar() ?? 0);

            return stats;
        }

        public List<TaskHistoryEntry> GetTaskActivityLog(int taskId)
        {
            var history = new List<TaskHistoryEntry>();
            using var conn = new SqlConnection(Conn);
            string query = @"SELECT h.*, u.Username FROM TaskHistory h 
                           LEFT JOIN AppUsers u ON h.UserId = u.Id 
                           WHERE h.TaskId=@tid 
                           ORDER BY h.CreatedAt DESC";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@tid", taskId);
            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                history.Add(new TaskHistoryEntry
                {
                    HistoryId = reader.GetInt32(0),
                    TaskId = reader.GetInt32(1),
                    Id = reader.GetInt32(2),
                    Action = reader.GetString(3),
                    OldValue = reader.IsDBNull(4) ? null : reader.GetString(4),
                    NewValue = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CreatedAt = reader.GetDateTime(6),
                    UserName = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }
            return history;
        }

        public bool LogTaskAction(int taskId, int userId, string action, string? oldValue = null, string? newValue = null)
        {
            using var conn = new SqlConnection(Conn);
            string query = @"INSERT INTO TaskHistory (TaskId, UserId, Action, OldValue, NewValue) 
                           VALUES (@tid, @uid, @action, @old, @new)";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@tid", taskId);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@action", action);
            cmd.Parameters.AddWithValue("@old", (object?)oldValue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@new", (object?)newValue ?? DBNull.Value);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
