using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagerApi.Services;

namespace TaskManagerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;
        private readonly AuthService _authService;

        public DashboardController(DashboardService dashboardService, AuthService authService)
        {
            _dashboardService = dashboardService;
            _authService = authService;
        }

        private int CurrentUserId()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            return _authService.GetUserId(username!);
        }

        [HttpGet("stats")]
        public IActionResult GetDashboardStats()
        {
            var stats = _dashboardService.GetDashboardStats(CurrentUserId());
            return Ok(stats);
        }

        [HttpGet("activity/{taskId}")]
        public IActionResult GetTaskActivity(int taskId)
        {
            var activity = _dashboardService.GetTaskActivityLog(taskId);
            return Ok(activity);
        }
    }
}
