namespace BusinessLogic.DTOs.Request;

public class CreateScheduleEventRequest
{
    public int ClassSectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
    public string? Location { get; set; }
    public string? OnlineUrl { get; set; }
    public int? TeacherId { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateOnly? RecurrenceStartDate { get; set; }
    public DateOnly? RecurrenceEndDate { get; set; }
    public int CreatedBy { get; set; }
}
