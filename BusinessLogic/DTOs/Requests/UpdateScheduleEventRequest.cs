namespace BusinessLogic.DTOs.Request;

public class UpdateScheduleEventRequest
{
    public long ScheduleEventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
    public string? Location { get; set; }
    public string? OnlineUrl { get; set; }
    public int? TeacherId { get; set; }
    public int UpdatedBy { get; set; }
    public string? Reason { get; set; }
}
