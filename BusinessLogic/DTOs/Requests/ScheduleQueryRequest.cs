using BusinessObject.Enum;

namespace BusinessLogic.DTOs.Request;

public class ScheduleQueryRequest
{
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public int SemesterId { get; set; }
    public CalendarViewMode ViewMode { get; set; } = CalendarViewMode.WEEK;
    public DateOnly? AnchorDate { get; set; }
    public DateOnly? RangeStart { get; set; }
    public DateOnly? RangeEnd { get; set; }
}
