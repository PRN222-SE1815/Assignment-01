namespace BusinessLogic.DTOs.Response;

public class PlanSimulationSectionDto
{
    public int ClassSectionId { get; set; }
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}
