using BusinessLogic.DTOs.Response;

namespace Web.Models.Student;

public class MyCoursesViewModel
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public IReadOnlyList<EnrollmentDto> Courses { get; set; } = Array.Empty<EnrollmentDto>();
}
