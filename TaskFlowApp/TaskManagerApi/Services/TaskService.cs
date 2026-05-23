using Microsoft.Data.SqlClient;
using TaskManagerApi.Models;
using Microsoft.Extensions.Logging;
using TaskManagerApi.DTOs;

namespace TaskManagerApi.Services
{
    public class TaskService
    {
        private static readonly HashSet<string> AllowedSortColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            "CreatedAt", "DueDate", "Priority", "Status", "Title"
        };

        private static readonly HashSet<string> AllowedSortDirections = new(StringComparer.OrdinalIgnoreCase)
        {
            "ASC", "DESC"
        };

        private readonly IConfiguration _config;
        private readonly ILogger<TaskService> _logger;
        private readonly TagService _tagService;

        public TaskService(IConfiguration config, ILogger<TaskService> logger, TagService tagService)
        {
            _config = config;
            _logger = logger;
            _tagService = tagService;
            EnsureTaskTableSchema();
        }

        private string Conn =>
            _config.GetConnectionString("BTConnection")!;

        private void EnsureTaskTableSchema()
        {
            using SqlConnection conn = new SqlConnection(Conn);
            string alter = @"
                IF COL_LENGTH('dbo.Tasks', 'Status') IS NULL
                    ALTER TABLE dbo.Tasks ADD Status NVARCHAR(20) DEFAULT 'Pending';
                IF COL_LENGTH('dbo.Tasks', 'Category') IS NULL
                    ALTER TABLE dbo.Tasks ADD Category NVARCHAR(50) DEFAULT 'General';
                IF COL_LENGTH('dbo.Tasks', 'IsDeleted') IS NULL
                    ALTER TABLE dbo.Tasks ADD IsDeleted BIT DEFAULT 0;
                IF COL_LENGTH('dbo.Tasks', 'DueDate') IS NULL
                    ALTER TABLE dbo.Tasks ADD DueDate DATETIME NULL;
                IF COL_LENGTH('dbo.Tasks', 'DueTime') IS NULL
                    ALTER TABLE dbo.Tasks ADD DueTime TIME NULL;
                IF COL_LENGTH('dbo.Tasks', 'CompletedAt') IS NULL
                    ALTER TABLE dbo.Tasks ADD CompletedAt DATETIME NULL;
                IF COL_LENGTH('dbo.Tasks', 'CreatedAt') IS NULL
                    ALTER TABLE dbo.Tasks ADD CreatedAt DATETIME DEFAULT GETDATE();
                IF COL_LENGTH('dbo.Tasks', 'UpdatedAt') IS NULL
                    ALTER TABLE dbo.Tasks ADD UpdatedAt DATETIME DEFAULT GETDATE();
                IF COL_LENGTH('dbo.Tasks', 'ProjectId') IS NULL
                    ALTER TABLE dbo.Tasks ADD ProjectId INT NULL;
            ";

            try
            {
                conn.Open();
                using SqlCommand cmd = new SqlCommand(alter, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not ensure Tasks schema. This is safe to ignore if schema is already correct.");
            }
        }

        public List<TaskItem> GetTasks(int userId)
        {
            _logger.LogDebug($"Fetching tasks for userId: {userId}");
            List<TaskItem> tasks = new();

            using SqlConnection conn = new SqlConnection(Conn);

            string query = @"
                SELECT TaskId, UserId, Title, Description,
                       ISNULL(Status, 'Pending') AS Status,
                       ISNULL(Priority, 'Medium') AS Priority,
                       ISNULL(Category, 'General') AS Category,
                     DueDate, DueTime, CreatedAt, ProjectId,
                       CompletedAt, IsDeleted
                FROM Tasks
                WHERE UserId=@uid AND ISNULL(IsDeleted,0)=0
                ORDER BY CreatedAt DESC";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            conn.Open();
            {
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tasks.Add(new TaskItem
                    {
                        TaskId = (int)reader["TaskId"],
                        UserId = (int)reader["UserId"],
                        Title = reader["Title"]?.ToString() ?? string.Empty,
                        Description = reader["Description"]?.ToString() ?? string.Empty,
                        Status = reader["Status"]?.ToString() ?? "Pending",
                        Priority = reader["Priority"]?.ToString() ?? "Medium",
                        Category = reader["Category"]?.ToString() ?? "General",
                        DueDate = reader["DueDate"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["DueDate"],
                        DueTime = reader["DueTime"] == DBNull.Value ? null : (TimeSpan?)reader["DueTime"],
                        CreatedAt = reader["CreatedAt"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["CreatedAt"],
                        ProjectId = reader["ProjectId"] == DBNull.Value ? null : (int?)reader["ProjectId"],
                        CompletedAt = reader["CompletedAt"] == DBNull.Value ? null : (DateTime?)reader["CompletedAt"],
                        IsDeleted = reader["IsDeleted"] == DBNull.Value ? false : (bool)reader["IsDeleted"]
                    });
                }
            }

            PopulateTaskTagIds(conn, tasks, userId);

            _logger.LogDebug($"Retrieved {tasks.Count} tasks for userId: {userId}");
            return tasks;
        }

        public PagedTaskResponse GetTasksFiltered(int userId, TaskFilterRequest filter)
        {
            _logger.LogDebug($"Fetching filtered tasks for userId: {userId}");
            List<TaskItem> tasks = new();

            using SqlConnection conn = new SqlConnection(Conn);

            var whereClauses = new List<string> { "UserId=@uid", "IsDeleted=0" };
            
            if (!string.IsNullOrEmpty(filter.Status))
                whereClauses.Add("Status=@status");
            if (!string.IsNullOrEmpty(filter.Priority))
                whereClauses.Add("Priority=@priority");
            if (!string.IsNullOrEmpty(filter.Category))
                whereClauses.Add("Category=@category");
            if (!string.IsNullOrEmpty(filter.SearchTerm))
                whereClauses.Add("(Title LIKE @search OR Description LIKE @search)");
            if (filter.FromDate.HasValue)
                whereClauses.Add("DueDate >= @fromDate");
            if (filter.ToDate.HasValue)
                whereClauses.Add("DueDate <= @toDate");

            string whereClause = string.Join(" AND ", whereClauses);
            var safeSortBy = AllowedSortColumns.Contains(filter.SortBy) ? filter.SortBy : "CreatedAt";
            var safeSortOrder = AllowedSortDirections.Contains(filter.SortOrder) ? filter.SortOrder.ToUpperInvariant() : "DESC";
            string orderBy = $"ORDER BY [{safeSortBy}] {safeSortOrder}";

            string countQuery = $"SELECT COUNT(*) FROM Tasks WHERE {whereClause}";
            string query = $@"
                SELECT TaskId, UserId, Title, Description,
                       ISNULL(Status, 'Pending') AS Status,
                       ISNULL(Priority, 'Medium') AS Priority,
                       ISNULL(Category, 'General') AS Category,
                        DueDate, DueTime, CreatedAt, ProjectId, CompletedAt, IsDeleted
                FROM Tasks
                WHERE {whereClause}
                {orderBy}
                OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            SqlCommand countCmd = new SqlCommand(countQuery, conn);
            SqlCommand cmd = new SqlCommand(query, conn);

            countCmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@offset", (filter.Page - 1) * filter.PageSize);
            cmd.Parameters.AddWithValue("@pageSize", filter.PageSize);

            if (!string.IsNullOrEmpty(filter.Status))
            {
                countCmd.Parameters.AddWithValue("@status", filter.Status);
                cmd.Parameters.AddWithValue("@status", filter.Status);
            }
            if (!string.IsNullOrEmpty(filter.Priority))
            {
                countCmd.Parameters.AddWithValue("@priority", filter.Priority);
                cmd.Parameters.AddWithValue("@priority", filter.Priority);
            }
            if (!string.IsNullOrEmpty(filter.Category))
            {
                countCmd.Parameters.AddWithValue("@category", filter.Category);
                cmd.Parameters.AddWithValue("@category", filter.Category);
            }
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                string searchParam = $"%{filter.SearchTerm}%";
                countCmd.Parameters.AddWithValue("@search", searchParam);
                cmd.Parameters.AddWithValue("@search", searchParam);
            }
            if (filter.FromDate.HasValue)
            {
                countCmd.Parameters.AddWithValue("@fromDate", filter.FromDate.Value);
                cmd.Parameters.AddWithValue("@fromDate", filter.FromDate.Value);
            }
            if (filter.ToDate.HasValue)
            {
                countCmd.Parameters.AddWithValue("@toDate", filter.ToDate.Value);
                cmd.Parameters.AddWithValue("@toDate", filter.ToDate.Value);
            }

            conn.Open();
            
            int totalCount = (int)countCmd.ExecuteScalar();
            {
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tasks.Add(new TaskItem
                    {
                        TaskId = (int)reader["TaskId"],
                        UserId = (int)reader["UserId"],
                        Title = reader["Title"]?.ToString() ?? string.Empty,
                        Description = reader["Description"]?.ToString() ?? string.Empty,
                        Status = reader["Status"]?.ToString() ?? "Pending",
                        Priority = reader["Priority"]?.ToString() ?? "Medium",
                        Category = reader["Category"]?.ToString() ?? "General",
                        DueDate = reader["DueDate"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["DueDate"],
                        DueTime = reader["DueTime"] == DBNull.Value ? null : (TimeSpan?)reader["DueTime"],
                        CreatedAt = reader["CreatedAt"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["CreatedAt"],
                        ProjectId = reader["ProjectId"] == DBNull.Value ? null : (int?)reader["ProjectId"],
                        CompletedAt = reader["CompletedAt"] == DBNull.Value ? null : (DateTime?)reader["CompletedAt"],
                        IsDeleted = reader["IsDeleted"] == DBNull.Value ? false : (bool)reader["IsDeleted"]
                    });
                }
            }

            PopulateTaskTagIds(conn, tasks, userId);

            return new PagedTaskResponse
            {
                Tasks = tasks,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        public TaskStatsResponse GetTaskStats(int userId)
        {
            _logger.LogDebug($"Fetching task stats for userId: {userId}");
            
            using SqlConnection conn = new SqlConnection(Conn);

            string query = @"
                SELECT 
                    COUNT(*) as Total,
                    SUM(CASE WHEN Status='Completed' THEN 1 ELSE 0 END) as Completed,
                    SUM(CASE WHEN Status='Pending' THEN 1 ELSE 0 END) as Pending,
                    SUM(CASE WHEN Status='In Progress' THEN 1 ELSE 0 END) as InProgress,
                    SUM(CASE WHEN DueDate < GETDATE() AND Status != 'Completed' THEN 1 ELSE 0 END) as Overdue,
                    SUM(CASE WHEN Priority='High' OR Priority='Critical' THEN 1 ELSE 0 END) as HighPriority
                FROM Tasks
                WHERE UserId=@uid AND IsDeleted=0";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            conn.Open();
            var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new TaskStatsResponse
                {
                    TotalTasks = reader.GetInt32(0),
                    CompletedTasks = reader.GetInt32(1),
                    PendingTasks = reader.GetInt32(2),
                    InProgressTasks = reader.GetInt32(3),
                    OverdueTasks = reader.GetInt32(4),
                    HighPriorityTasks = reader.GetInt32(5)
                };
            }

            return new TaskStatsResponse();
        }

        public void UpdateTask(int id, TaskItem task, int userId)
        {
            using SqlConnection conn = new SqlConnection(Conn);
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                string query = @"
                    UPDATE Tasks
                    SET Title=@t,
                        Description=@d,
                        Status=@s,
                        Priority=@p,
                        Category=@c,
                        ProjectId=@projectId,
                        DueDate=@due,
                        DueTime=@dueTime,
                        CompletedAt=@completed
                    WHERE TaskId=@id AND UserId=@uid";

                using var cmd = new SqlCommand(query, conn, transaction);

                cmd.Parameters.AddWithValue("@t", task.Title);
                cmd.Parameters.AddWithValue("@d", task.Description);
                cmd.Parameters.AddWithValue("@s", task.Status);
                cmd.Parameters.AddWithValue("@p", task.Priority);
                cmd.Parameters.AddWithValue("@c", task.Category);
                cmd.Parameters.AddWithValue("@projectId", (object?)task.ProjectId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@due", task.DueDate);
                cmd.Parameters.AddWithValue("@dueTime", (object?)task.DueTime ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@completed",
                    task.Status == "Completed" ? (object)DateTime.UtcNow : DBNull.Value);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@uid", userId);

                if (cmd.ExecuteNonQuery() == 0)
                    throw new InvalidOperationException("Task not found or access denied.");

                if (!_tagService.ReplaceTaskTags(conn, transaction, id, userId, task.TagIds))
                    throw new InvalidOperationException("Failed to update task tags.");

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void AddTask(TaskItem task, int userId)
        {
            _logger.LogDebug($"Adding new task for userId: {userId}, Title: {task.Title}");
            using SqlConnection conn = new SqlConnection(Conn);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                string query = @"
                    INSERT INTO Tasks
                    (Title, Description, Status, Priority, Category, DueDate, DueTime, ProjectId, UserId)
                    VALUES (@t,@d,@s,@p,@c,@due,@dueTime,@projectId,@uid);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using var cmd = new SqlCommand(query, conn, transaction);

                cmd.Parameters.AddWithValue("@t", task.Title);
                cmd.Parameters.AddWithValue("@d", task.Description);
                cmd.Parameters.AddWithValue("@s", task.Status);
                cmd.Parameters.AddWithValue("@p", task.Priority);
                cmd.Parameters.AddWithValue("@c", task.Category);
                cmd.Parameters.AddWithValue("@due", task.DueDate);
                cmd.Parameters.AddWithValue("@dueTime", (object?)task.DueTime ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@projectId", (object?)task.ProjectId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@uid", userId);

                var insertedTaskId = Convert.ToInt32(cmd.ExecuteScalar());

                if (!_tagService.ReplaceTaskTags(conn, transaction, insertedTaskId, userId, task.TagIds))
                    throw new InvalidOperationException("Failed to save task tags.");

                transaction.Commit();
                task.TaskId = insertedTaskId;
                task.UserId = userId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            _logger.LogDebug($"Task added successfully for userId: {userId}");
        }

        public void SoftDelete(int taskId, int userId)
        {
            _logger.LogDebug($"Soft deleting taskId: {taskId}");
            using SqlConnection conn = new SqlConnection(Conn);

            string query =
                "UPDATE Tasks SET IsDeleted=1 WHERE TaskId=@id AND UserId=@uid";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", taskId);
            cmd.Parameters.AddWithValue("@uid", userId);

            conn.Open();
            cmd.ExecuteNonQuery();
            _logger.LogDebug($"Task {taskId} deleted successfully");
        }

        private void PopulateTaskTagIds(SqlConnection conn, List<TaskItem> tasks, int userId)
        {
            if (tasks.Count == 0)
                return;

            var taskIds = tasks.Select(task => task.TaskId).Distinct().ToList();
            var parameterNames = taskIds.Select((_, index) => $"@task{index}").ToList();

            var query = $@"
                SELECT tt.TaskId, tt.TagId
                FROM TaskTags tt
                INNER JOIN Tasks task ON task.TaskId = tt.TaskId
                INNER JOIN Tags tag ON tag.TagId = tt.TagId
                WHERE task.UserId=@uid AND tag.UserId=@uid AND tt.TaskId IN ({string.Join(",", parameterNames)})";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);

            for (var index = 0; index < taskIds.Count; index++)
            {
                cmd.Parameters.AddWithValue(parameterNames[index], taskIds[index]);
            }

            var taskTagIds = new Dictionary<int, List<int>>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var taskId = reader.GetInt32(0);
                var tagId = reader.GetInt32(1);

                if (!taskTagIds.TryGetValue(taskId, out var ids))
                {
                    ids = new List<int>();
                    taskTagIds[taskId] = ids;
                }

                ids.Add(tagId);
            }

            foreach (var task in tasks)
            {
                task.TagIds = taskTagIds.TryGetValue(task.TaskId, out var ids)
                    ? ids.Distinct().ToList()
                    : new List<int>();
            }
        }
    }
}
