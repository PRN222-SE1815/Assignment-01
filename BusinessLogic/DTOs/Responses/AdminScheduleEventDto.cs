namespace BusinessLogic.DTOs.Response;

public class AdminScheduleEventDto
{
    public long ScheduleEventId { get; set; }
    public int ClassSectionId { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? OnlineUrl { get; set; }
    public int? TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public int? RecurrenceId { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateOnly? RecurrenceStartDate { get; set; }
    public DateOnly? RecurrenceEndDate { get; set; }
}
