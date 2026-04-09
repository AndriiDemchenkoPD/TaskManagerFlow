using Microsoft.Data.SqlClient;
using TaskManagerApi.Models;

namespace TaskManagerApi.Services
{
    public class ProjectService
    {
        private readonly IConfiguration _config;
        private string Conn => _config.GetConnectionString("BTConnection")!;

        public ProjectService(IConfiguration config)
        {
            _config = config;
        }

        public List<Project> GetUserProjects(int userId)
        {
            var projects = new List<Project>();
            using var conn = new SqlConnection(Conn);
            string query = "SELECT * FROM Projects WHERE UserId=@uid AND IsDeleted=0 ORDER BY CreatedAt DESC";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                projects.Add(new Project
                {
                    ProjectId = reader.GetInt32(0),
                    Id = reader.GetInt32(1),
                    ProjectName = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Color = reader.GetString(4),
                    CreatedAt = reader.GetDateTime(5),
                    IsDeleted = reader.GetBoolean(6)
                });
            }
            return projects;
        }

        public bool CreateProject(Project project)
        {
            using var conn = new SqlConnection(Conn);
            string query = @"INSERT INTO Projects (UserId, ProjectName, Description, Color) 
                           VALUES (@uid, @name, @desc, @color)";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", project.Id);
            cmd.Parameters.AddWithValue("@name", project.ProjectName);
            cmd.Parameters.AddWithValue("@desc", (object?)project.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@color", project.Color);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateProject(Project project)
        {
            using var conn = new SqlConnection(Conn);
            string query = @"UPDATE Projects SET ProjectName=@name, Description=@desc, Color=@color 
                           WHERE ProjectId=@id AND UserId=@uid";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", project.ProjectName);
            cmd.Parameters.AddWithValue("@desc", (object?)project.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@color", project.Color);
            cmd.Parameters.AddWithValue("@id", project.ProjectId);
            cmd.Parameters.AddWithValue("@uid", project.Id);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteProject(int projectId, int userId)
        {
            using var conn = new SqlConnection(Conn);
            string query = "UPDATE Projects SET IsDeleted=1 WHERE ProjectId=@id AND UserId=@uid";
            var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", projectId);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
