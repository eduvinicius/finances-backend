using MyFinances.Domain.Enums;

namespace MyFinances.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public NotificationTargetingMode TargetingMode { get; set; }
        public NotificationDeliveryChannel DeliveryChannel { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserNotification> UserNotifications { get; set; } = [];
    }
}
