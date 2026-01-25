using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;

        public NotificationService(INotificationRepository notificationRepository, IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        public async Task SendNotificationAsync(int? senderUserId, List<int> receiverUserIds, string title, string message)
        {
            var notification = new Notification
            {
                SenderUserId = senderUserId,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            var savedNotification = await _notificationRepository.CreateNotificationAsync(notification);

            foreach (var receiverId in receiverUserIds)
            {
                await _notificationRepository.AddRecipientAsync(savedNotification.NotificationId, receiverId);
            }
        }

        public async Task<List<NotificationResponse>> GetUserNotificationsAsync(int userId, int skip = 0, int take = 20)
        {
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, skip, take);
            var responses = new List<NotificationResponse>();

            foreach (var notification in notifications)
            {
                var sender = notification.SenderUserId.HasValue 
                    ? await _userRepository.GetByIdAsync(notification.SenderUserId.Value) 
                    : null;

                responses.Add(new NotificationResponse
                {
                    NotificationId = notification.NotificationId,
                    SenderUserId = notification.SenderUserId,
                    SenderName = sender?.FullName ?? "System",
                    Title = notification.Title ?? "",
                    Message = notification.Message ?? "",
                    CreatedAt = notification.CreatedAt ?? DateTime.Now,
                    IsRead = false,
                    ReadAt = null
                });
            }

            return responses;
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            return await _notificationRepository.MarkAsReadAsync(notificationId, userId);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }
    }
}
