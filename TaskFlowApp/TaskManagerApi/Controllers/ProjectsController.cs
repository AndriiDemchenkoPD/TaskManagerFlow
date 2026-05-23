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
    public class ProjectsController : ControllerBase
    {
        private readonly ProjectService _projectService;
        private readonly AuthService _authService;

        public ProjectsController(ProjectService projectService, AuthService authService)
        {
            _projectService = projectService;
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
        public IActionResult GetProjects()
        {
            var projects = _projectService.GetUserProjects(CurrentUserId());
            return Ok(projects);
        }

        [HttpPost]
        public IActionResult CreateProject([FromBody] Project project)
        {
            project.Id = CurrentUserId();
            project.CreatedAt = DateTime.Now;
            if (_projectService.CreateProject(project))
                return Ok(new { message = "Project created successfully" });
            return BadRequest(new { message = "Failed to create project" });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProject(int id, [FromBody] Project project)
        {
            project.ProjectId = id;
            project.Id = CurrentUserId();
            if (_projectService.UpdateProject(project))
                return Ok(new { message = "Project updated successfully" });
            return BadRequest(new { message = "Failed to update project" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProject(int id)
        {
            if (_projectService.DeleteProject(id, CurrentUserId()))
                return Ok(new { message = "Project deleted successfully" });
            return BadRequest(new { message = "Failed to delete project" });
        }
    }
}
