using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<long> CreateNotificationAsync(Notification notification, IReadOnlyCollection<int> recipientUserIds);
}
