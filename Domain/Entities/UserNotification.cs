namespace MyFinances.Domain.Entities
{
    public class UserNotification
    {
        public int Id { get; set; }
        public int NotificationId { get; set; }
        public Notification Notification { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
