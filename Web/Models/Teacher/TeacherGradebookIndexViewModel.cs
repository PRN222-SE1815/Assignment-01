using BusinessLogic.DTOs.Response;

namespace Web.Models.Teacher;

public class TeacherGradebookIndexViewModel
{
    public IReadOnlyList<ClassSectionDto> Sections { get; set; } = Array.Empty<ClassSectionDto>();
}
