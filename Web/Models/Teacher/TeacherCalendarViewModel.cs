using BusinessObject.Enum;

namespace Web.Models.Teacher;

public class TeacherCalendarViewModel
{
    public CalendarViewMode ViewMode { get; set; } = CalendarViewMode.WEEK;
    public DateOnly AnchorDate { get; set; }
    public DateOnly RangeStart { get; set; }
    public DateOnly RangeEnd { get; set; }
    public Dictionary<DateOnly, IReadOnlyList<TeacherScheduleOccurrenceViewModel>> OccurrencesByDay { get; set; } = new();
}

public class TeacherScheduleOccurrenceViewModel
{
    public string OccurrenceId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
    public DateTime StartAtLocal { get; set; }
    public DateTime EndAtLocal { get; set; }
    public string? Location { get; set; }
    public string? OnlineUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsOverride { get; set; }
}
