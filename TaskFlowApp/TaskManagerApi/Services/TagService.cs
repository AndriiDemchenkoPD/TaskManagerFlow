using Microsoft.Data.SqlClient;
using TaskManagerApi.Models;

namespace TaskManagerApi.Services
{
    public class TagService
    {
        private readonly IConfiguration _config;
        private string Conn => _config.GetConnectionString("BTConnection")!;

        public TagService(IConfiguration config)
        {
            _config = config;
        }

        public List<Tag> GetUserTags(int userId)
        {
            var tags = new List<Tag>();
            using var conn = new SqlConnection(Conn);
            string query = "SELECT * FROM Tags WHERE UserId=@uid ORDER BY TagName";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tags.Add(new Tag
                {
                    TagId = reader.GetInt32(0),
                    Id = reader.GetInt32(1),
                    TagName = reader.GetString(2),
                    Color = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4)
                });
            }
            return tags;
        }

        public bool CreateTag(Tag tag)
        {
            using var conn = new SqlConnection(Conn);
            string query = "INSERT INTO Tags (UserId, TagName, Color) VALUES (@uid, @name, @color)";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", tag.Id);
            cmd.Parameters.AddWithValue("@name", tag.TagName);
            cmd.Parameters.AddWithValue("@color", tag.Color);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateTag(Tag tag)
        {
            using var conn = new SqlConnection(Conn);
            string query = @"UPDATE Tags
                           SET TagName=@name, Color=@color
                           WHERE TagId=@id AND UserId=@uid";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", tag.TagId);
            cmd.Parameters.AddWithValue("@uid", tag.Id);
            cmd.Parameters.AddWithValue("@name", tag.TagName);
            cmd.Parameters.AddWithValue("@color", tag.Color);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteTag(int tagId, int userId)
        {
            using var conn = new SqlConnection(Conn);
            conn.Open();

            var unlinkCmd = new SqlCommand(@"
                DELETE tt
                FROM TaskTags tt
                INNER JOIN BT_Tasks t ON t.TaskId = tt.TaskId
                WHERE tt.TagId=@tagId AND t.UserId=@uid", conn);
            unlinkCmd.Parameters.AddWithValue("@tagId", tagId);
            unlinkCmd.Parameters.AddWithValue("@uid", userId);
            unlinkCmd.ExecuteNonQuery();

            var deleteCmd = new SqlCommand("DELETE FROM Tags WHERE TagId=@id AND UserId=@uid", conn);
            deleteCmd.Parameters.AddWithValue("@id", tagId);
            deleteCmd.Parameters.AddWithValue("@uid", userId);
            return deleteCmd.ExecuteNonQuery() > 0;
        }

        public List<Tag> GetTaskTags(int taskId, int userId)
        {
            var tags = new List<Tag>();
            using var conn = new SqlConnection(Conn);
            string query = @"SELECT DISTINCT t.* FROM Tags t 
                           INNER JOIN TaskTags tt ON t.TagId = tt.TagId 
                           INNER JOIN BT_Tasks task ON task.TaskId = tt.TaskId
                           WHERE tt.TaskId=@tid AND task.UserId=@uid AND t.UserId=@uid";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@tid", taskId);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tags.Add(new Tag
                {
                    TagId = reader.GetInt32(0),
                    Id = reader.GetInt32(1),
                    TagName = reader.GetString(2),
                    Color = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4)
                });
            }
            return tags;
        }

        public bool AddTagToTask(int taskId, int tagId, int userId)
        {
            using var conn = new SqlConnection(Conn);
            conn.Open();

            if (!TaskAndTagBelongToUser(conn, taskId, tagId, userId))
                return false;

            string query = @"
                IF NOT EXISTS (SELECT 1 FROM TaskTags WHERE TaskId=@tid AND TagId=@tagid)
                    INSERT INTO TaskTags (TaskId, TagId) VALUES (@tid, @tagid)";

            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@tid", taskId);
            cmd.Parameters.AddWithValue("@tagid", tagId);

            try
            {
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
        }

        public bool RemoveTagFromTask(int taskId, int tagId, int userId)
        {
            using var conn = new SqlConnection(Conn);
            conn.Open();
            if (!TaskAndTagBelongToUser(conn, taskId, tagId, userId))
                return false;

            string query = @"
                DELETE tt
                FROM TaskTags tt
                INNER JOIN BT_Tasks task ON task.TaskId = tt.TaskId
                INNER JOIN Tags tag ON tag.TagId = tt.TagId
                WHERE tt.TaskId=@tid AND tt.TagId=@tagid AND task.UserId=@uid AND tag.UserId=@uid";

            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@tid", taskId);
            cmd.Parameters.AddWithValue("@tagid", tagId);
            cmd.Parameters.AddWithValue("@uid", userId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool ReplaceTaskTags(SqlConnection conn, SqlTransaction transaction, int taskId, int userId, IEnumerable<int>? tagIds)
        {
            var normalizedTagIds = (tagIds ?? Enumerable.Empty<int>())
                .Where(tagId => tagId > 0)
                .Distinct()
                .ToList();

            if (!TaskExistsForUser(conn, transaction, taskId, userId))
                return false;

            using (var deleteCmd = new SqlCommand(@"
                DELETE tt
                FROM TaskTags tt
                INNER JOIN BT_Tasks task ON task.TaskId = tt.TaskId
                WHERE tt.TaskId=@tid AND task.UserId=@uid", conn, transaction))
            {
                deleteCmd.Parameters.AddWithValue("@tid", taskId);
                deleteCmd.Parameters.AddWithValue("@uid", userId);
                deleteCmd.ExecuteNonQuery();
            }

            if (normalizedTagIds.Count == 0)
                return true;

            var validTagIds = new HashSet<int>();
            var parameterNames = new List<string>();

            for (var index = 0; index < normalizedTagIds.Count; index++)
            {
                parameterNames.Add($"@tag{index}");
            }

            var validationQuery = $@"
                SELECT TagId
                FROM Tags
                WHERE UserId=@uid AND TagId IN ({string.Join(",", parameterNames)})";

            using (var validationCmd = new SqlCommand(validationQuery, conn, transaction))
            {
                validationCmd.Parameters.AddWithValue("@uid", userId);

                for (var index = 0; index < normalizedTagIds.Count; index++)
                {
                    validationCmd.Parameters.AddWithValue(parameterNames[index], normalizedTagIds[index]);
                }

                using var reader = validationCmd.ExecuteReader();
                while (reader.Read())
                {
                    validTagIds.Add(reader.GetInt32(0));
                }
            }

            if (validTagIds.Count != normalizedTagIds.Count)
                return false;

            foreach (var tagId in normalizedTagIds)
            {
                using var insertCmd = new SqlCommand(@"
                    IF NOT EXISTS (SELECT 1 FROM TaskTags WHERE TaskId=@tid AND TagId=@tagid)
                        INSERT INTO TaskTags (TaskId, TagId) VALUES (@tid, @tagid)", conn, transaction);
                insertCmd.Parameters.AddWithValue("@tid", taskId);
                insertCmd.Parameters.AddWithValue("@tagid", tagId);
                insertCmd.ExecuteNonQuery();
            }

            return true;
        }

        private bool TaskAndTagBelongToUser(SqlConnection conn, int taskId, int tagId, int userId)
        {
            const string query = @"
                SELECT COUNT(1)
                FROM BT_Tasks task
                INNER JOIN Tags tag ON tag.TagId = @tagid
                WHERE task.TaskId = @tid
                  AND task.UserId = @uid
                  AND tag.UserId = @uid";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@tid", taskId);
            cmd.Parameters.AddWithValue("@tagid", tagId);
            cmd.Parameters.AddWithValue("@uid", userId);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private bool TaskExistsForUser(SqlConnection conn, SqlTransaction transaction, int taskId, int userId)
        {
            const string query = @"
                SELECT COUNT(1)
                FROM BT_Tasks
                WHERE TaskId=@tid AND UserId=@uid";

            using var cmd = new SqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("@tid", taskId);
            cmd.Parameters.AddWithValue("@uid", userId);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
    }
}
