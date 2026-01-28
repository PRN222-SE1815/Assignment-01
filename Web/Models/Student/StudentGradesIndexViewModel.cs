using BusinessLogic.DTOs.Response;

namespace Web.Models.Student;

public class StudentGradesIndexViewModel
{
    public IReadOnlyList<GradebookDto> Sections { get; set; } = Array.Empty<GradebookDto>();
}
