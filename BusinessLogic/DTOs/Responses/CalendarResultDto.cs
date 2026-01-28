namespace BusinessLogic.DTOs.Response;

public class CalendarResultDto
{
    public DateOnly RangeStart { get; set; }
    public DateOnly RangeEnd { get; set; }
    public IReadOnlyList<ScheduleOccurrenceDto> Occurrences { get; set; } = Array.Empty<ScheduleOccurrenceDto>();
}
