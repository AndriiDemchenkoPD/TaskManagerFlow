using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagerApi.Models
{
    [Table("Tasks", Schema = "dbo")]
    public class TaskItem
    {
        public int TaskId { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Status { get; set; }
        public required string Priority { get; set; } = "Medium";
        public required string Category { get; set; } = "General";
        public DateTime DueDate { get; set; }
        public TimeSpan? DueTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? ProjectId { get; set; }
        public int UserId { get; set; }
        public bool IsDeleted { get; set; }
        public List<int> TagIds { get; set; } = new();
    }
}
