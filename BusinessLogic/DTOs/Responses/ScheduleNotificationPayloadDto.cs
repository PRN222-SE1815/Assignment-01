namespace BusinessLogic.DTOs.Response;

public class ScheduleNotificationPayloadDto
{
    public int ClassSectionId { get; set; }
    public long ScheduleEventId { get; set; }
    public DateOnly OccurrenceDate { get; set; }
    public DateTime FromStartAtUtc { get; set; }
    public DateTime FromEndAtUtc { get; set; }
    public DateTime? ToStartAtUtc { get; set; }
    public DateTime? ToEndAtUtc { get; set; }
    public string? Reason { get; set; }
    public string LinkRoute { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
}
