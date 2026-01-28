namespace BusinessLogic.DTOs.Response;

public class EnrollmentDto
{
    public int EnrollmentId { get; set; }
    public int ClassSectionId { get; set; }
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
    public string? TeacherName { get; set; }
    public int CreditsSnapshot { get; set; }
    public string Status { get; set; } = string.Empty;
}
