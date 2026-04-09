using Microsoft.Data.SqlClient;
using TaskManagerApi.Models;
using System.Data;

namespace TaskManagerApi.APIs
{
    public class TaskResult
    {
        public bool IsSuccessful { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
    }

    public class TaskMst
    {
        private readonly IConfiguration _config;
        private readonly int _userId;
        private readonly string _userSign;
        private readonly string _connectionString;

        public TaskMst(IConfiguration config, int userId, string userSign)
        {
            _config = config;
            _userId = userId;
            _userSign = userSign;
            _connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        }

        public TaskResult GetTasks()
        {
            try
            {
                var tasks = new List<TaskItem>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM BT_Tasks WHERE UserId=@UserId AND IsDeleted=0 ORDER BY CreatedAt DESC";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", _userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tasks.Add(new TaskItem
                                {
                                    TaskId = (int)reader["TaskId"],
                                    Title = (string)reader["Title"],
                                    Description = reader["Description"] != DBNull.Value ? (string)reader["Description"] : "",
                                    Status = (string)reader["Status"],
                                    Priority = (string)reader["Priority"],
                                    Category = "General",
                                    CreatedAt = (DateTime)reader["CreatedAt"]
                                });
                            }
                        }
                    }
                }
                return new TaskResult { IsSuccessful = true, Data = tasks };
            }
            catch (Exception ex)
            {
                return new TaskResult { IsSuccessful = false, Message = ex.Message };
            }
        }

        public TaskResult SaveTask(TaskItem task)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string query = @"INSERT INTO BT_Tasks 
                                (Title, Description, Status, Priority, UserId, CreatedAt, CreatedBy, IsDeleted)
                                VALUES (@Title, @Description, @Status, @Priority, @UserId, @CreatedAt, @CreatedBy, 0)";
                            
                            using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Title", task.Title ?? "");
                                cmd.Parameters.AddWithValue("@Description", task.Description ?? "");
                                cmd.Parameters.AddWithValue("@Status", task.Status ?? "Pending");
                                cmd.Parameters.AddWithValue("@Priority", task.Priority ?? "Medium");
                                cmd.Parameters.AddWithValue("@UserId", _userId);
                                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                                cmd.Parameters.AddWithValue("@CreatedBy", _userSign);
                                
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return new TaskResult { IsSuccessful = true, Message = "Task saved successfully" };
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new TaskResult { IsSuccessful = false, Message = ex.Message };
            }
        }
    }
}
