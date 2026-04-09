using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagerApi.Models;
using TaskManagerApi.Services;

namespace TaskManagerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TagsController : ControllerBase
    {
        private readonly TagService _tagService;
        private readonly AuthService _authService;

        public TagsController(TagService tagService, AuthService authService)
        {
            _tagService = tagService;
            _authService = authService;
        }

        private int CurrentUserId()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            return _authService.GetUserId(username!);
        }

        [HttpGet]
        public IActionResult GetTags()
        {
            var tags = _tagService.GetUserTags(CurrentUserId());
            return Ok(tags);
        }

        [HttpPost]
        public IActionResult CreateTag([FromBody] Tag tag)
        {
            tag.Id = CurrentUserId();
            if (_tagService.CreateTag(tag))
                return Ok(new { message = "Tag created successfully" });
            return BadRequest(new { message = "Tag already exists" });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTag(int id, [FromBody] Tag tag)
        {
            tag.TagId = id;
            tag.Id = CurrentUserId();
            if (_tagService.UpdateTag(tag))
                return Ok(new { message = "Tag updated successfully" });
            return BadRequest(new { message = "Failed to update tag" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTag(int id)
        {
            if (_tagService.DeleteTag(id, CurrentUserId()))
                return Ok(new { message = "Tag deleted successfully" });
            return BadRequest(new { message = "Failed to delete tag" });
        }

        [HttpGet("task/{taskId}")]
        public IActionResult GetTaskTags(int taskId)
        {
            var tags = _tagService.GetTaskTags(taskId, CurrentUserId());
            return Ok(tags);
        }

        [HttpPost("task/{taskId}/tag/{tagId}")]
        public IActionResult AddTagToTask(int taskId, int tagId)
        {
            if (_tagService.AddTagToTask(taskId, tagId, CurrentUserId()))
                return Ok(new { message = "Tag added to task" });
            return BadRequest(new { message = "Failed to add tag" });
        }

        [HttpDelete("task/{taskId}/tag/{tagId}")]
        public IActionResult RemoveTagFromTask(int taskId, int tagId)
        {
            if (_tagService.RemoveTagFromTask(taskId, tagId, CurrentUserId()))
                return Ok(new { message = "Tag removed from task" });
            return BadRequest(new { message = "Failed to remove tag" });
        }
    }
}
