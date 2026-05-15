using MyFinances.Domain.Entities;

namespace MyFinances.App.Services.Notifications
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(CreateNotificationRequest request);

        Task<List<UserNotification>> GetUserInboxAsync();

        Task MarkAsReadAsync(int userNotificationId);

        Task MarkAllAsReadAsync();

        Task DeleteUserNotificationAsync(int userNotificationId);

        Task<List<Notification>> GetAdminNotificationHistoryAsync();
    }
}
