using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SchoolManagementDbContext _context;

        public NotificationRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task AddRecipientAsync(int notificationId, int receiverUserId)
        {
            var recipient = new NotificationRecipient
            {
                NotificationId = notificationId,
                ReceiverUserId = receiverUserId,
                IsRead = false
            };

            _context.NotificationRecipients.Add(recipient);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int skip = 0, int take = 20)
        {
            return await _context.NotificationRecipients
                .Where(nr => nr.ReceiverUserId == userId)
                .OrderByDescending(nr => nr.Notification.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(nr => nr.Notification)
                .Include(n => n.SenderUser)
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var recipient = await _context.NotificationRecipients
                .FirstOrDefaultAsync(nr => nr.NotificationId == notificationId && nr.ReceiverUserId == userId);

            if (recipient == null || recipient.IsRead == true)
                return false;

            recipient.IsRead = true;
            recipient.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.NotificationRecipients
                .CountAsync(nr => nr.ReceiverUserId == userId && nr.IsRead != true);
        }
    }
}