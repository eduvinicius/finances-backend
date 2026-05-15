using MyFinances.Domain.Enums;

namespace MyFinances.Api.DTOs.Notifications
{
    public record CreateNotificationDto
    {
        public required string Title { get; init; }
        public required string Body { get; init; }
        public NotificationTargetingMode TargetingMode { get; init; }
        public NotificationDeliveryChannel DeliveryChannel { get; init; }
        public List<Guid>? TargetUserIds { get; init; }
    }

    public record NotificationResponseDto
    {
        public int Id { get; init; }
        public required string Title { get; init; }
        public required string Body { get; init; }
        public required string TargetingMode { get; init; }
        public required string DeliveryChannel { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
