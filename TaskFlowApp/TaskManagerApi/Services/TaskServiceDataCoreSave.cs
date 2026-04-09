using TaskManagerApi.Models;
using Microsoft.Extensions.Logging;
using System.Data;
using Microsoft.Data.SqlClient;

namespace TaskManagerApi.Services
{
    public class TaskServiceDataCoreSave
    {
        private readonly IConfiguration _config;
        private readonly ILogger<TaskServiceDataCoreSave> _logger;

        public TaskServiceDataCoreSave(IConfiguration config, ILogger<TaskServiceDataCoreSave> logger)
        {
            _config = config;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _config.GetConnectionString("BTConnection") ?? "";
        }

        public bool SaveTaskWithAudit(TaskItem task, int userId, int timeZoneNo = 2)
        {
            _logger.LogDebug($"Saving task for userId: {userId}");

            try
            {
                using SqlConnection conn = new SqlConnection(GetConnectionString());
                conn.Open();
                using SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string query = @"INSERT INTO BT_Tasks 
                        (Title, Description, Status, Priority, Category, UserId, DueDate, CreatedAt, CreatedBy, IsDeleted)
                        VALUES (@Title, @Description, @Status, @Priority, @Category, @UserId, @DueDate, @CreatedAt, @CreatedBy, 0)";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Title", task.Title ?? "");
                        cmd.Parameters.AddWithValue("@Description", task.Description ?? "");
                        cmd.Parameters.AddWithValue("@Status", task.Status ?? "Pending");
                        cmd.Parameters.AddWithValue("@Priority", task.Priority ?? "Medium");
                        cmd.Parameters.AddWithValue("@Category", task.Category ?? "General");
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@DueDate", task.DueDate);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        cmd.Parameters.AddWithValue("@CreatedBy", userId);
                        
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    _logger.LogDebug($"Task saved successfully with audit trail");
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError($"Save error: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                return false;
            }
        }
    }
}
