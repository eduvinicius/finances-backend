using MyFinances.Domain.Entities;
using MyFinances.Domain.Enums;

namespace MyFinances.App.Services.Notifications
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(
            string title,
            string body,
            NotificationTargetingMode targetingMode,
            NotificationDeliveryChannel channel,
            List<Guid>? targetUserIds);

        Task<List<UserNotification>> GetUserInboxAsync();

        Task MarkAsReadAsync(int userNotificationId);

        Task DeleteUserNotificationAsync(int userNotificationId);

        Task<List<Notification>> GetAdminNotificationHistoryAsync();
    }
}
