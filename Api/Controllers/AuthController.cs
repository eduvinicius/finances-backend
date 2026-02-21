using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFinances.Api.DTOs;
using MyFinances.App.Services.Interfaces;

namespace MyFinances.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService, ICurrentUserService currentUserService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly ICurrentUserService _currentUserService = currentUserService;

        [HttpGet("current-user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = _currentUserService.UserId;
            var user = await _authService.GetUserByIdAsync(userId);

            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            await _authService.RegisterAsync(dto);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var token = await _authService.LoginAsync(dto);
            return Ok(token);
        }

        [Authorize]
        [HttpPost("update-profile-image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            var userId = _currentUserService.UserId;

            var imageUrl = await _authService
                .UploadProfileImageAsync(userId, file);

            return Ok(new { imageUrl });
        }

        [Authorize]
        [HttpPut("update-user")]
        public async Task<IActionResult> EditProfile(UpdateUserDto user)
        {
            var userId = _currentUserService.UserId;
            var updatedUser = await _authService.UpdateUserAsync(userId, user);
            return Ok(updatedUser);
        }

        [Authorize]
        [HttpGet("profile-image")]
        public async Task<IActionResult> GetProfileImage()
        {
            var userId = _currentUserService.UserId;
            var imageStream = await _authService.GetProfileImageAsync(userId);

            return File(imageStream, "image/jpeg", enableRangeProcessing: true);
        }
    }
}
