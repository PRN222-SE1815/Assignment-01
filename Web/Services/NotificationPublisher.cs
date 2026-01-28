using BusinessLogic.DTOs.Response;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Services;

public sealed class NotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationPublisher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishScheduleNotificationAsync(IReadOnlyCollection<int> recipientUserIds, ScheduleNotificationPayloadDto payload)
    {
        if (recipientUserIds.Count == 0)
        {
            return Task.CompletedTask;
        }

        var tasks = recipientUserIds
            .Distinct()
            .Select(userId => _hubContext.Clients.User(userId.ToString())
                .SendAsync("scheduleNotification", payload));

        return Task.WhenAll(tasks);
    }
}
