using BusinessLogic.DTOs.Response;

namespace BusinessLogic.Services.Interfaces;

public interface INotificationPublisher
{
    Task PublishScheduleNotificationAsync(IReadOnlyCollection<int> recipientUserIds, ScheduleNotificationPayloadDto payload);
}
