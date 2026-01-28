using BusinessLogic.DTOs.Request;
using BusinessLogic.DTOs.Response;

namespace BusinessLogic.Services.Interfaces;

public interface IEnrollmentService
{
    Task<IReadOnlyList<ClassSectionDto>> GetOpenSectionsAsync(int? semesterId = null);
    Task<IReadOnlyList<EnrollmentDto>> GetMyCoursesAsync(int studentId, int? semesterId = null);
    Task<OperationResult> RegisterAsync(RegisterCourseRequest request);
    Task<OperationResult> DropAsync(DropRequest request);
    Task<OperationResult> WithdrawAsync(WithdrawRequest request);
    Task<OperationResult<PlanSimulationResultDto>> SimulatePlanAsync(int studentId, int semesterId, IReadOnlyList<int> plannedSectionIds);
}
