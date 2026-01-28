namespace BusinessLogic.DTOs.Response;

public class GradebookMatrixDto
{
    public GradebookDto Gradebook { get; set; } = new();
    public IReadOnlyList<GradeItemDto> Items { get; set; } = Array.Empty<GradeItemDto>();
    public IReadOnlyList<StudentRowDto> Students { get; set; } = Array.Empty<StudentRowDto>();
    public IReadOnlyList<GradeEntryCellDto> Entries { get; set; } = Array.Empty<GradeEntryCellDto>();
    public StatsDto? Stats { get; set; }
}
