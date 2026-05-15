using MyFinances.App.DTOs;
using MyFinances.App.Services.Admin;

namespace MyFinances.Api.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUserController(IAdminUserService adminUserService) : ControllerBase
    {
        private readonly IAdminUserService _adminUserService = adminUserService;

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string? name)
        {
            var result = await _adminUserService.SearchUsersByNameAsync(name ?? string.Empty);
            return Ok(result);
        }

        [HttpPost("getAll")]
        public async Task<IActionResult> GetUsers([FromBody] AdminUserFilterDto filters)
        {
            var result = await _adminUserService.GetUsersAsync(filters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _adminUserService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpPatch("{id}/role")]
        public async Task<IActionResult> ChangeUserRole(Guid id, [FromBody] ChangeUserRoleDto dto)
        {
            await _adminUserService.ChangeUserRoleAsync(id, dto.Role);
            return Ok();
        }

        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(Guid id)
        {
            await _adminUserService.DeactivateUserAsync(id);
            return Ok();
        }

        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivateUser(Guid id)
        {
            await _adminUserService.ActivateUserAsync(id);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            await _adminUserService.DeleteUserAsync(id);
            return Ok();
        }
    }
}
