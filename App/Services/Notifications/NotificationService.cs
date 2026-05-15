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

        public async Task CreateNotificationAsync(CreateNotificationRequest request)
        {
            if (request.TargetingMode is NotificationTargetingMode.SingleUser or NotificationTargetingMode.SelectedUsers
                && (request.TargetUserIds is null || request.TargetUserIds.Count == 0))
            {
                throw new BadRequestException("É necessário informar ao menos um usuário destinatário para o modo de targeting selecionado.");
            }

            if (request.TargetingMode == NotificationTargetingMode.SingleUser && request.TargetUserIds!.Count != 1)
                throw new BadRequestException("SingleUser targeting mode requires exactly one target user ID.");

            var notification = new Notification
            {
                Title = request.Title,
                Body = request.Body,
                TargetingMode = request.TargetingMode,
                DeliveryChannel = request.DeliveryChannel,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepo.AddNotificationAsync(notification);

            var recipients = await FanOutUserNotificationsAsync(notification, request.TargetingMode, request.TargetUserIds);

            await _unitOfWork.SaveChangesAsync();

            if (request.DeliveryChannel is NotificationDeliveryChannel.Email or NotificationDeliveryChannel.Both)
                await SendEmailNotificationsAsync(recipients, request.Title, request.Body);
        }

        private async Task<List<User>> FanOutUserNotificationsAsync(
            Notification notification,
            NotificationTargetingMode targetingMode,
            List<Guid>? targetUserIds)
        {
            var expiresAt = DateTime.UtcNow.AddDays(30);

            switch (targetingMode)
            {
                case NotificationTargetingMode.AllUsers:
                    var allUsers = await _notificationRepo.GetAllUsersAsync();
                    foreach (var user in allUsers)
                    {
                        await _notificationRepo.AddUserNotificationAsync(new UserNotification
                        {
                            Notification = notification,
                            UserId = user.Id,
                            ExpiresAt = expiresAt
                        });
                    }
                    return allUsers;

                case NotificationTargetingMode.SingleUser:
                    var singleUser = await _notificationRepo.GetUsersByIdsAsync(targetUserIds!);
                    if (singleUser.Count == 0)
                        throw new BadRequestException("O usuário selecionado não existe, está inativo ou é um administrador.");
                    await _notificationRepo.AddUserNotificationAsync(new UserNotification
                    {
                        Notification = notification,
                        UserId = singleUser[0].Id,
                        ExpiresAt = expiresAt
                    });
                    return singleUser;

                case NotificationTargetingMode.SelectedUsers:
                    var distinctIds = targetUserIds!.Distinct().ToList();
                    var selectedUsers = await _notificationRepo.GetUsersByIdsAsync(distinctIds);
                    if (selectedUsers.Count == 0)
                        throw new BadRequestException("Nenhum dos usuários selecionados existe, está ativo ou possui perfil de usuário comum.");
                    if (selectedUsers.Count != distinctIds.Count)
                        throw new BadRequestException("Um ou mais usuários selecionados são administradores, inexistentes ou inativos.");
                    foreach (var user in selectedUsers)
                    {
                        await _notificationRepo.AddUserNotificationAsync(new UserNotification
                        {
                            Notification = notification,
                            UserId = user.Id,
                            ExpiresAt = expiresAt
                        });
                    }
                    return selectedUsers;

                default:
                    throw new BadRequestException($"Unsupported targeting mode: {targetingMode}.");
            }
        }

        private async Task SendEmailNotificationsAsync(List<User> recipients, string title, string body)
        {
            foreach (var recipient in recipients)
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
