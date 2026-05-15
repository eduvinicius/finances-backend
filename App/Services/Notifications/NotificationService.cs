using MyFinances.App.Abstractions;
using MyFinances.Domain.Entities;
using MyFinances.Domain.Enums;
using MyFinances.Domain.Exceptions;

namespace MyFinances.App.Services.Notifications
{
    public class NotificationService(
        INotificationRepository notificationRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        ILogger<NotificationService> logger) : INotificationService
    {
        private readonly INotificationRepository _notificationRepo = notificationRepo;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IEmailService _emailService = emailService;
        private readonly ILogger<NotificationService> _logger = logger;

        public async Task CreateNotificationAsync(
            string title,
            string body,
            NotificationTargetingMode targetingMode,
            NotificationDeliveryChannel channel,
            List<Guid>? targetUserIds)
        {
            if (targetingMode is NotificationTargetingMode.SingleUser or NotificationTargetingMode.SelectedUsers
                && (targetUserIds is null || targetUserIds.Count == 0))
            {
                throw new BadRequestException("É necessário informar ao menos um usuário destinatário para o modo de targeting selecionado.");
            }

            if (targetingMode == NotificationTargetingMode.SingleUser && targetUserIds!.Count != 1)
                throw new BadRequestException("SingleUser targeting mode requires exactly one target user ID.");

            var notification = new Notification
            {
                Title = title,
                Body = body,
                TargetingMode = targetingMode,
                DeliveryChannel = channel,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepo.AddNotificationAsync(notification);

            var expiresAt = DateTime.UtcNow.AddDays(30);

            List<User>? targetedUsers = null;

            switch (targetingMode)
            {
                case NotificationTargetingMode.AllUsers:
                    targetedUsers = await _notificationRepo.GetAllUsersAsync();
                    foreach (var user in targetedUsers)
                    {
                        await _notificationRepo.AddUserNotificationAsync(new UserNotification
                        {
                            Notification = notification,
                            UserId = user.Id,
                            ExpiresAt = expiresAt
                        });
                    }
                    break;

                case NotificationTargetingMode.SingleUser:
                    await _notificationRepo.AddUserNotificationAsync(new UserNotification
                    {
                        Notification = notification,
                        UserId = targetUserIds![0],
                        ExpiresAt = expiresAt
                    });
                    break;

                case NotificationTargetingMode.SelectedUsers:
                    foreach (var userId in targetUserIds!)
                    {
                        await _notificationRepo.AddUserNotificationAsync(new UserNotification
                        {
                            Notification = notification,
                            UserId = userId,
                            ExpiresAt = expiresAt
                        });
                    }
                    break;
            }

            await _unitOfWork.SaveChangesAsync();

            if (channel is NotificationDeliveryChannel.Email or NotificationDeliveryChannel.Both)
            {
                if (targetingMode == NotificationTargetingMode.AllUsers)
                {
                    // targetedUsers was already fetched during fan-out — reuse it
                }
                else
                {
                    // Fetch the targeted users to obtain their email addresses
                    targetedUsers = await _notificationRepo.GetUsersByIdsAsync(targetUserIds!);
                }

                foreach (var recipient in targetedUsers ?? [])
                {
                    try
                    {
                        await _emailService.SendEmailAsync(recipient.Email, recipient.FullName, title, body);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notification email to {Email} (UserId: {UserId})", recipient.Email, recipient.Id);
                    }
                }
            }
        }

        public async Task<List<UserNotification>> GetUserInboxAsync()
        {
            var userId = _currentUserService.UserId;
            return await _notificationRepo.GetUserInboxAsync(userId);
        }

        public async Task MarkAsReadAsync(int userNotificationId)
        {
            var userId = _currentUserService.UserId;
            var userNotification = await _notificationRepo.GetUserNotificationAsync(userNotificationId, userId)
                ?? throw new NotFoundException("Notificação não encontrada.");

            userNotification.IsRead = true;
            userNotification.ReadAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync()
        {
            var userId = _currentUserService.UserId;
            var unread = await _notificationRepo.GetUnreadUserNotificationsAsync(userId);

            var now = DateTime.UtcNow;
            foreach (var userNotification in unread)
            {
                userNotification.IsRead = true;
                userNotification.ReadAt = now;
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteUserNotificationAsync(int userNotificationId)
        {
            var userId = _currentUserService.UserId;
            var userNotification = await _notificationRepo.GetUserNotificationAsync(userNotificationId, userId)
                ?? throw new NotFoundException("Notificação não encontrada.");

            userNotification.IsDeleted = true;
            userNotification.DeletedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetAdminNotificationHistoryAsync()
        {
            return await _notificationRepo.GetAllNotificationsAsync();
        }
    }
}
