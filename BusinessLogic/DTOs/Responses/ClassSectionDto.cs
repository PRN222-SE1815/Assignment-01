namespace BusinessLogic.DTOs.Response;

public class ClassSectionDto
{
    public int ClassSectionId { get; set; }
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public string SectionCode { get; set; } = string.Empty;
    public string? TeacherName { get; set; }
    public int Credits { get; set; }
    public bool IsOpen { get; set; }
    public int MaxCapacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public string? Room { get; set; }
    public string? OnlineUrl { get; set; }
}
