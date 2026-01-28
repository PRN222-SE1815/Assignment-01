using BusinessLogic.DTOs.Response;

namespace Web.Models.Admin;

public class ScheduleEventListViewModel
{
    public int ClassSectionId { get; set; }
    public IReadOnlyList<AdminScheduleEventDto> Events { get; set; } = Array.Empty<AdminScheduleEventDto>();
}
