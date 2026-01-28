using BusinessLogic.DTOs.Response;

namespace BusinessLogic.Services.Interfaces;

public interface IGradebookService
{
    Task<IReadOnlyList<ClassSectionDto>> GetTeacherSectionsAsync(int teacherId);
    Task<GradebookMatrixDto?> GetGradebookAsync(int classSectionId, int actorUserId);
    Task<OperationResult> SaveStructureAsync(int classSectionId, int actorUserId, IReadOnlyList<GradeItemDto> items);
    Task<OperationResult> SaveGradesAsync(int classSectionId, int actorUserId, IReadOnlyList<GradeEntryCellDto> entries, string? reason = null);
    Task<OperationResult> PublishAsync(int classSectionId, int actorUserId);
    Task<OperationResult> LockAsync(int classSectionId, int actorUserId);
    Task<OperationResult> ArchiveAsync(int classSectionId, int actorUserId);
}
