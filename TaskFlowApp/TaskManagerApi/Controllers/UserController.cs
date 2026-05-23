using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagerApi.Services;
using TaskManagerApi.Models;

namespace TaskManagerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;
        private readonly ILogger<UserController> _logger;

        public UserController(UserService userService, AuthService authService, ILogger<UserController> logger)
        {
            _userService = userService;
            _authService = authService;
            _logger = logger;
        }

        private int CurrentUserId()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
                throw new UnauthorizedAccessException("Invalid token");
            return _authService.GetUserId(username);
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            try
            {
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                
                if (string.IsNullOrEmpty(username))
                    return Unauthorized(new { message = "Invalid token - no username claim" });
                
                var userId = _authService.GetUserId(username);
                
                var user = _userService.GetUserById(userId);
                
                if (user == null)
                    return NotFound(new { message = "User not found" });

                return Ok(new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user profile");
                return StatusCode(500, new { message = "Failed to load profile" });
            }
        }

        [HttpPut("profile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = CurrentUserId();
                var success = _userService.UpdateUserProfile(userId, request.Username, request.Email);
                
                if (success)
                    return Ok(new { message = "Profile updated successfully" });
                else
                    return BadRequest(new { message = "Failed to update profile" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class UpdateProfileRequest
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
    }
}
