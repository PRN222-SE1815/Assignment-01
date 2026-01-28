using BusinessLogic.DTOs.Response;

namespace Web.Models.Student;

public class RegistrationIndexViewModel
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public IReadOnlyList<ClassSectionDto> Sections { get; set; } = Array.Empty<ClassSectionDto>();
}
