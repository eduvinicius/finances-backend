using MyFinances.Domain.Entities;

namespace MyFinances.App.Abstractions
{
    public interface INotificationRepository
    {
        Task AddNotificationAsync(Notification notification);
        Task AddUserNotificationAsync(UserNotification userNotification);
        Task<List<UserNotification>> GetUserInboxAsync(Guid userId);
        Task<UserNotification?> GetUserNotificationAsync(int userNotificationId, Guid userId);
        Task<List<Notification>> GetAllNotificationsAsync();
        Task<List<UserNotification>> GetUnreadUserNotificationsAsync(Guid userId);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetUsersByIdsAsync(List<Guid> userIds);
    }
}
