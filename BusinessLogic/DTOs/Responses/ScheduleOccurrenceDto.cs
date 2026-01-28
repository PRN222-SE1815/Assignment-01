namespace BusinessLogic.DTOs.Response;

public class ScheduleOccurrenceDto
{
    public string OccurrenceId { get; set; } = string.Empty;
    public long ScheduleEventId { get; set; }
    public int ClassSectionId { get; set; }
    public string SectionCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateOnly OccurrenceDate { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public string Timezone { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? OnlineUrl { get; set; }
    public int? TeacherId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsOverride { get; set; }
    public string? Reason { get; set; }
}
