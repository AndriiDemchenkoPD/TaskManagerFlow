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

        public UserController(UserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
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
                Console.WriteLine("GetProfile called");
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                Console.WriteLine($"Username from token: {username}");
                
                if (string.IsNullOrEmpty(username))
                    return Unauthorized(new { message = "Invalid token - no username claim" });
                
                var userId = _authService.GetUserId(username);
                Console.WriteLine($"UserId: {userId}");
                
                var user = _userService.GetUserById(userId);
                Console.WriteLine($"User found: {user?.Username}");
                
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
                Console.WriteLine($"Error in GetProfile: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = ex.Message, details = ex.StackTrace });
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
