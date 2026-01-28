using BusinessLogic.DTOs.Response;

namespace Web.Models.Teacher;

public class TeacherGradebookEditViewModel
{
    public GradebookDto Gradebook { get; set; } = new();
    public IReadOnlyList<GradeItemDto> Items { get; set; } = Array.Empty<GradeItemDto>();
    public IReadOnlyList<StudentRowDto> Students { get; set; } = Array.Empty<StudentRowDto>();
    public IReadOnlyList<GradeEntryCellDto> Entries { get; set; } = Array.Empty<GradeEntryCellDto>();
    public StatsDto? Stats { get; set; }

    public bool CanEdit => Gradebook.Status is "DRAFT" or "PUBLISHED";
    public bool CanPublish => Gradebook.Status == "DRAFT";
    public bool CanLock => Gradebook.Status is "DRAFT" or "PUBLISHED";
    public bool IsLocked => Gradebook.Status is "LOCKED" or "ARCHIVED";

    public decimal? GetScore(int gradeItemId, int enrollmentId)
    {
        return Entries.FirstOrDefault(e => e.GradeItemId == gradeItemId && e.EnrollmentId == enrollmentId)?.Score;
    }

    public string GetHistogramJson()
    {
        if (Stats?.Histogram == null || Stats.Histogram.Count == 0)
        {
            return "[]";
        }

        return "[" + string.Join(",", Stats.Histogram) + "]";
    }

    public string GetPieDataJson()
    {
        if (Stats == null)
        {
            return "[0,0,0]";
        }

        return $"[{Stats.AboveCount},{Stats.BelowCount},{Stats.NotGradedCount}]";
    }
}
