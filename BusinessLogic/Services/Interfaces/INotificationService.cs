using BusinessLogic.DTOs.Responses;

namespace BusinessLogic.Services.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(int? senderUserId, List<int> receiverUserIds, string title, string message);
    Task<List<NotificationResponse>> GetUserNotificationsAsync(int userId, int skip = 0, int take = 20);
    Task<bool> MarkAsReadAsync(int notificationId, int userId);
    Task<int> GetUnreadCountAsync(int userId);
}