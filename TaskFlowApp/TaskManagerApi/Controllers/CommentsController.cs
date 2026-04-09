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
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;
        private readonly AuthService _authService;

        public CommentsController(CommentService commentService, AuthService authService)
        {
            _commentService = commentService;
            _authService = authService;
        }

        private int CurrentUserId()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            return _authService.GetUserId(username!);
        }

        [HttpGet]
        public IActionResult GetComments(int taskId)
        {
            var comments = _commentService.GetTaskComments(taskId);
            return Ok(comments);
        }

        [HttpPost]
        public IActionResult AddComment(int taskId, [FromBody] TaskComment comment)
        {
            comment.TaskId = taskId;
            comment.Id = CurrentUserId();
            comment.CreatedAt = DateTime.Now;
            comment.UpdatedAt = DateTime.Now;
            if (_commentService.AddComment(comment))
                return Ok(new { message = "Comment added successfully" });
            return BadRequest(new { message = "Failed to add comment" });
        }

        [HttpPut("{commentId}")]
        public IActionResult UpdateComment(int taskId, int commentId, [FromBody] TaskComment comment)
        {
            if (_commentService.UpdateComment(commentId, comment.CommentText, CurrentUserId()))
                return Ok(new { message = "Comment updated successfully" });
            return BadRequest(new { message = "Failed to update comment" });
        }

        [HttpDelete("{commentId}")]
        public IActionResult DeleteComment(int taskId, int commentId)
        {
            if (_commentService.DeleteComment(commentId, CurrentUserId()))
                return Ok(new { message = "Comment deleted successfully" });
            return BadRequest(new { message = "Failed to delete comment" });
        }
    }
}
