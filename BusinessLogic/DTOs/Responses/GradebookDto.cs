namespace BusinessLogic.DTOs.Response;

public class GradebookDto
{
    public int GradeBookId { get; set; }
    public int ClassSectionId { get; set; }
    public int CourseId { get; set; }
    public string? CourseCode { get; set; }
    public string? CourseName { get; set; }
    public string? SectionCode { get; set; }
    public int SemesterId { get; set; }
    public string? SemesterName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
