using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerApi.Models;
using TaskManagerApi.Services;

namespace TaskManagerApi.Controllers
{
    [ApiController]
    [Route("api/tasks/{taskId}/[controller]")]
    [Authorize]
    public class SubtasksController : ControllerBase
    {
        private readonly SubtaskService _subtaskService;

        public SubtasksController(SubtaskService subtaskService)
        {
            _subtaskService = subtaskService;
        }

        [HttpGet]
        public IActionResult GetSubtasks(int taskId)
        {
            var subtasks = _subtaskService.GetSubtasks(taskId);
            var completionPercentage = _subtaskService.GetSubtaskCompletionPercentage(taskId);
            return Ok(new { subtasks, completionPercentage });
        }

        [HttpPost]
        public IActionResult CreateSubtask(int taskId, [FromBody] Subtask subtask)
        {
            subtask.ParentTaskId = taskId;
            subtask.CreatedAt = DateTime.Now;
            if (_subtaskService.CreateSubtask(subtask))
                return Ok(new { message = "Subtask created successfully" });
            return BadRequest(new { message = "Failed to create subtask" });
        }

        [HttpPut("{subtaskId}")]
        public IActionResult UpdateSubtaskStatus(int taskId, int subtaskId, [FromBody] Subtask subtask)
        {
            if (_subtaskService.UpdateSubtaskStatus(subtaskId, subtask.IsCompleted))
                return Ok(new { message = "Subtask updated successfully" });
            return BadRequest(new { message = "Failed to update subtask" });
        }

        [HttpDelete("{subtaskId}")]
        public IActionResult DeleteSubtask(int taskId, int subtaskId)
        {
            if (_subtaskService.DeleteSubtask(subtaskId))
                return Ok(new { message = "Subtask deleted successfully" });
            return BadRequest(new { message = "Failed to delete subtask" });
        }
    }
}
