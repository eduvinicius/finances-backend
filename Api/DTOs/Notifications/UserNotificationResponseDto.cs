namespace MyFinances.Api.DTOs.Notifications
{
    public record UserNotificationResponseDto
    {
        public int Id { get; init; }
        public int NotificationId { get; init; }
        public required string Title { get; init; }
        public required string Body { get; init; }
        public bool IsRead { get; init; }
        public DateTime? ReadAt { get; init; }
        public DateTime ExpiresAt { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
