using AutoMapper;
using MyFinances.Api.DTOs.Notifications;
using MyFinances.App.Services.Notifications;

namespace MyFinances.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController(
        INotificationService notificationService,
        IMapper mapper) : ControllerBase
    {
        private readonly INotificationService _notificationService = notificationService;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<IActionResult> GetInbox()
        {
            var userNotifications = await _notificationService.GetUserInboxAsync();
            var response = _mapper.Map<List<UserNotificationResponseDto>>(userNotifications);
            return Ok(response);
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserNotification(int id)
        {
            await _notificationService.DeleteUserNotificationAsync(id);
            return NoContent();
        }
    }
}
