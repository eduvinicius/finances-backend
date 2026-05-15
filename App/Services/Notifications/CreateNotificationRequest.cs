using MyFinances.Domain.Enums;

namespace MyFinances.App.Services.Notifications
{
    public record CreateNotificationRequest(
        string Title,
        string Body,
        NotificationTargetingMode TargetingMode,
        NotificationDeliveryChannel DeliveryChannel,
        List<Guid>? TargetUserIds);
}
