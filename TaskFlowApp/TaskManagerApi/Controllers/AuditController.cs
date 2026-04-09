using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagerApi.Services;

namespace TaskManagerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly AuditService _auditService;

        public AuditController(AuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet("task/{taskId}")]
        public IActionResult GetTaskHistory(int taskId)
        {
            var history = _auditService.GetTaskAuditHistory(taskId);
            return Ok(history);
        }
    }
}
