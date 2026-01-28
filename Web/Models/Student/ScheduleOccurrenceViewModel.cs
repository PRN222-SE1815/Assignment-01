namespace Web.Models.Student;

public class ScheduleOccurrenceViewModel
{
    public string Title { get; set; } = string.Empty;
    public DateTime StartAtLocal { get; set; }
    public DateTime EndAtLocal { get; set; }
    public string? Location { get; set; }
    public string? OnlineUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
}
