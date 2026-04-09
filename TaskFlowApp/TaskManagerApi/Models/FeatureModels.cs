using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagerApi.Models
{
    [Table("Projects")]
    public class Project
    {
        public int ProjectId { get; set; }
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#3B82F6";
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }

    [Table("Tags")]
    public class Tag
    {
        public int TagId { get; set; }
        public int Id { get; set; }
        public string TagName { get; set; } = string.Empty;
        public string Color { get; set; } = "#10B981";
        public DateTime CreatedAt { get; set; }
    }

    [Table("TaskComments")]
    public class TaskComment
    {
        public int CommentId { get; set; }
        public int TaskId { get; set; }
        public int Id { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? UserName { get; set; }
    }

    [Table("TaskAttachments")]
    public class TaskAttachment
    {
        public int AttachmentId { get; set; }
        public int TaskId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int? FileSize { get; set; }
        public int UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    [Table("Subtasks")]
    public class Subtask
    {
        public int SubtaskId { get; set; }
        public int ParentTaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    [Table("TaskReminders")]
    public class TaskReminder
    {
        public int ReminderId { get; set; }
        public int TaskId { get; set; }
        public int Id { get; set; }
        public DateTime ReminderTime { get; set; }
        public string ReminderType { get; set; } = "Email";
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }
    }

    [Table("UserPreferences")]
    public class UserPreference
    {
        public int PreferenceId { get; set; }
        public int Id { get; set; }
        public bool DarkMode { get; set; }
        public string DefaultView { get; set; } = "List";
        public bool NotificationsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    [Table("TaskHistory")]
    public class TaskHistoryEntry
    {
        public int HistoryId { get; set; }
        public int TaskId { get; set; }
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UserName { get; set; }
    }

    public class DashboardStats
    {
        public int TotalTasks { get; set; }
        public int CompletedToday { get; set; }
        public int OverdueTasks { get; set; }
        public int InProgressTasks { get; set; }
        public double CompletionPercentage { get; set; }
        public int TotalProjects { get; set; }
    }
}
