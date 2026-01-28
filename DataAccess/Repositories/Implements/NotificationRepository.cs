using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace DataAccess.Repositories.Implements;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly SchoolManagementDbContext _context;

    public NotificationRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task<long> CreateNotificationAsync(Notification notification, IReadOnlyCollection<int> recipientUserIds)
    {
        if (recipientUserIds.Count == 0)
        {
            return 0;
        }

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var recipients = recipientUserIds
            .Distinct()
            .Select(userId => new NotificationRecipient
            {
                NotificationId = notification.NotificationId,
                UserId = userId
            })
            .ToList();

        _context.NotificationRecipients.AddRange(recipients);
        await _context.SaveChangesAsync();

        return notification.NotificationId;
    }
}
