namespace BusinessLogic.DTOs.Request;

public class UpdateScheduleStatusRequest
{
    public long ScheduleEventId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int UpdatedBy { get; set; }
    public string? Reason { get; set; }
}
