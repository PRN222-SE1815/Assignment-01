using BusinessLogic.DTOs.Response;

namespace BusinessLogic.Services.Interfaces;

public interface IStudentGradeService
{
    Task<IReadOnlyList<GradebookDto>> GetMyGradeSectionsAsync(int studentId);
    Task<GradebookMatrixDto?> GetMyGradeDetailsAsync(int studentId, int classSectionId);
}
