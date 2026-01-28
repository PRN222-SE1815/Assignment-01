using BusinessObject.Enum;

namespace Web.Models.Student;

public class StudentCalendarViewModel
{
    public CalendarViewMode ViewMode { get; set; } = CalendarViewMode.WEEK;
    public DateOnly AnchorDate { get; set; }
    public DateOnly RangeStart { get; set; }
    public DateOnly RangeEnd { get; set; }
    public Dictionary<DateOnly, IReadOnlyList<StudentScheduleOccurrenceViewModel>> OccurrencesByDay { get; set; } = new();
}

public class StudentScheduleOccurrenceViewModel
{
    public string OccurrenceId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateTime StartAtLocal { get; set; }
    public DateTime EndAtLocal { get; set; }
    public string? Location { get; set; }
    public string? OnlineUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsOverride { get; set; }
}
