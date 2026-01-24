using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<Notification> CreateNotificationAsync(Notification notification);
    Task AddRecipientAsync(int notificationId, int receiverUserId);
    Task<List<Notification>> GetUserNotificationsAsync(int userId, int skip = 0, int take = 20);
    Task<bool> MarkAsReadAsync(int notificationId, int userId);
    Task<int> GetUnreadCountAsync(int userId);
}