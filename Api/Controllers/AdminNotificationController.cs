using AutoMapper;
using MyFinances.Api.DTOs.Notifications;
using MyFinances.App.Services.Notifications;

namespace MyFinances.Api.Controllers
{
    [ApiController]
    [Route("api/admin/notifications")]
    [Authorize(Roles = "Admin")]
    public class AdminNotificationController(
        INotificationService notificationService,
        IMapper mapper) : ControllerBase
    {
        private readonly INotificationService _notificationService = notificationService;
        private readonly IMapper _mapper = mapper;

        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            await _notificationService.CreateNotificationAsync(
                dto.Title,
                dto.Body,
                dto.TargetingMode,
                dto.DeliveryChannel,
                dto.TargetUserIds);

            return Ok();
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> GetHistory()
        {
            var notifications = await _notificationService.GetAdminNotificationHistoryAsync();
            var response = _mapper.Map<IEnumerable<NotificationResponseDto>>(notifications);
            return Ok(response);
        }
    }
}
