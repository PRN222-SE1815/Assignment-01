namespace BusinessLogic.DTOs.Request;

public class CreateScheduleOverrideRequest
{
    public long ScheduleEventId { get; set; }
    public int RecurrenceId { get; set; }
    public DateOnly OriginalDate { get; set; }
    public string OverrideType { get; set; } = string.Empty;
    public DateTime? NewStartAtUtc { get; set; }
    public DateTime? NewEndAtUtc { get; set; }
    public string? NewLocation { get; set; }
    public int? NewTeacherId { get; set; }
    public string? Reason { get; set; }
    public int ActorUserId { get; set; }
}
