namespace Web.Models.Student;

public class ScheduleViewModel
{
    public DateOnly RangeStart { get; set; }
    public DateOnly RangeEnd { get; set; }
    public IReadOnlyList<ScheduleOccurrenceViewModel> Occurrences { get; set; } = Array.Empty<ScheduleOccurrenceViewModel>();
}
