namespace BusinessLogic.DTOs.Response;

public class PlanSimulationResultDto
{
    public int SemesterId { get; set; }
    public int TotalCredits { get; set; }
    public IReadOnlyList<PlanSimulationSectionDto> Sections { get; set; } = Array.Empty<PlanSimulationSectionDto>();
}
