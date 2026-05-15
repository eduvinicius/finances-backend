using Microsoft.EntityFrameworkCore;
using MyFinances.App.Abstractions;
using MyFinances.Domain.Entities;
using MyFinances.Domain.Enums;
using MyFinances.Infrastructure.Data;

namespace MyFinances.Infrastructure.Repositories
{
    public class NotificationRepository(FinanceDbContext context) : INotificationRepository
    {
        private readonly FinanceDbContext _context = context;

        public async Task AddNotificationAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
        }

        public async Task AddUserNotificationAsync(UserNotification userNotification)
        {
            await _context.UserNotifications.AddAsync(userNotification);
        }

        public async Task<List<UserNotification>> GetUserInboxAsync(Guid userId)
        {
            return await _context.UserNotifications
                .Where(un =>
                    un.UserId == userId &&
                    !un.IsDeleted &&
                    un.ExpiresAt > DateTime.UtcNow)
                .Include(un => un.Notification)
                .AsNoTracking()
                .OrderByDescending(un => un.Notification.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserNotification?> GetUserNotificationAsync(int userNotificationId, Guid userId)
        {
            return await _context.UserNotifications
                .FirstOrDefaultAsync(un =>
                    un.Id == userNotificationId &&
                    un.UserId == userId &&
                    !un.IsDeleted &&
                    un.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<List<Notification>> GetAllNotificationsAsync()
        {
            return await _context.Notifications
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        // AsNoTracking intentionally omitted — caller mutates the returned entities.
        public async Task<List<UserNotification>> GetUnreadUserNotificationsAsync(Guid userId)
        {
            return await _context.UserNotifications
                .Where(un =>
                    un.UserId == userId &&
                    !un.IsRead &&
                    !un.IsDeleted &&
                    un.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive && u.Role == UserRole.User)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<User>> GetUsersByIdsAsync(List<Guid> userIds)
        {
            return await _context.Users
                .Where(u => userIds.Contains(u.Id) && u.IsActive && u.Role == UserRole.User)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
