using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        private readonly AuthService _authService;

        public SubtasksController(SubtaskService subtaskService, AuthService authService)
        {
            _subtaskService = subtaskService;
            _authService = authService;
        }

        private int CurrentUserId()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                throw new UnauthorizedAccessException("Invalid token");
            return _authService.GetUserId(username);
        }

        [HttpGet]
        public IActionResult GetSubtasks(int taskId)
        {
            var userId = CurrentUserId();
            var subtasks = _subtaskService.GetSubtasks(taskId, userId);
            var completionPercentage = _subtaskService.GetSubtaskCompletionPercentage(taskId, userId);
            return Ok(new { subtasks, completionPercentage });
        }

        [HttpPost]
        public IActionResult CreateSubtask(int taskId, [FromBody] Subtask subtask)
        {
            if (subtask == null || string.IsNullOrWhiteSpace(subtask.Title))
                return BadRequest(new { message = "Subtask title is required" });

            subtask.ParentTaskId = taskId;
            subtask.CreatedAt = DateTime.Now;
            if (_subtaskService.CreateSubtask(subtask, CurrentUserId()))
                return Ok(new { message = "Subtask created successfully" });
            return BadRequest(new { message = "Failed to create subtask" });
        }

        [HttpPut("{subtaskId}")]
        public IActionResult UpdateSubtaskStatus(int taskId, int subtaskId, [FromBody] Subtask subtask)
        {
            if (_subtaskService.UpdateSubtaskStatus(subtaskId, subtask.IsCompleted, CurrentUserId()))
                return Ok(new { message = "Subtask updated successfully" });
            return BadRequest(new { message = "Failed to update subtask" });
        }

        [HttpDelete("{subtaskId}")]
        public IActionResult DeleteSubtask(int taskId, int subtaskId)
        {
            if (_subtaskService.DeleteSubtask(subtaskId, CurrentUserId()))
                return Ok(new { message = "Subtask deleted successfully" });
            return BadRequest(new { message = "Failed to delete subtask" });
        }
    }
}
