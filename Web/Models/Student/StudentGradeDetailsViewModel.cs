using BusinessLogic.DTOs.Response;

namespace Web.Models.Student;

public class StudentGradeDetailsViewModel
{
    public GradebookDto Gradebook { get; set; } = new();
    public IReadOnlyList<GradeItemDto> Items { get; set; } = Array.Empty<GradeItemDto>();
    public StudentRowDto Student { get; set; } = new();
    public IReadOnlyList<GradeEntryCellDto> Entries { get; set; } = Array.Empty<GradeEntryCellDto>();

    public decimal? GetScore(int gradeItemId)
    {
        return Entries.FirstOrDefault(e => e.GradeItemId == gradeItemId)?.Score;
    }
}
