using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagerApi.Models;
using TaskManagerApi.Services;
using TaskManagerApi.APIs;

namespace TaskManagerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskDataCoreSaveController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AuthService _authService;

        public TaskDataCoreSaveController(IConfiguration config, AuthService authService)
        {
            _config = config;
            _authService = authService;
        }

        private int CurrentUserId()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                throw new UnauthorizedAccessException("Invalid token");
            return _authService.GetUserId(username);
        }

        private string GetUserSign()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            return $"{username} (User)";
        }

        [HttpGet]
        public JsonResult GetTasks()
        {
            TaskResult r;
            bool isSuccess = false;
            string strMessage = string.Empty, res = string.Empty;

            try
            {
                r = (new TaskMst(_config, CurrentUserId(), GetUserSign())).GetTasks();

                if (!r.IsSuccessful)
                    throw new Exception(r.Message ?? "Unknown error");

                if (r.IsSuccessful)
                {
                    isSuccess = true;
                    res = r.Data?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                strMessage = ex.Message.ToString();
            }

            var jsonResult = new
            {
                isSuccess = isSuccess,
                strMessage = strMessage,
                res = res
            };

            return new JsonResult(jsonResult);
        }

        [HttpPost]
        public JsonResult SaveTask(TaskItem task)
        {
            TaskResult r;
            bool isSuccess = false;
            string strMessage = string.Empty, res = string.Empty;

            try
            {
                task.UserId = CurrentUserId();
                task.CreatedAt = DateTime.Now;

                r = (new TaskMst(_config, CurrentUserId(), GetUserSign())).SaveTask(task);

                if (!r.IsSuccessful)
                    throw new Exception(r.Message ?? "Unknown error");

                if (r.IsSuccessful)
                {
                    isSuccess = true;
                    res = r.Data?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                strMessage = ex.Message.ToString();
            }

            var jsonResult = new
            {
                isSuccess = isSuccess,
                strMessage = strMessage,
                res = res
            };

            return new JsonResult(jsonResult);
        }

        [HttpPut("{id}")]
        public JsonResult UpdateTask(int id, TaskItem task)
        {
            TaskResult r;
            bool isSuccess = false;
            string strMessage = string.Empty, res = string.Empty;

            try
            {
                task.TaskId = id;
                task.UserId = CurrentUserId();

                r = (new TaskMst(_config, CurrentUserId(), GetUserSign())).SaveTask(task);

                if (!r.IsSuccessful)
                    throw new Exception(r.Message ?? "Unknown error");

                if (r.IsSuccessful)
                {
                    isSuccess = true;
                    res = r.Data?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                strMessage = ex.Message.ToString();
            }

            var jsonResult = new
            {
                isSuccess = isSuccess,
                strMessage = strMessage,
                res = res
            };

            return new JsonResult(jsonResult);
        }
    }
}
