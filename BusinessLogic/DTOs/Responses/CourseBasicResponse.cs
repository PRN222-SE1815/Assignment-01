namespace BusinessLogic.DTOs.Responses;

public class CourseBasicResponse
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string? Semester { get; set; }
}

