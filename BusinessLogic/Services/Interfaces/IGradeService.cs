using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;

namespace BusinessLogic.Services.Interfaces;

public interface IGradeService
{
    Task<List<GradeResponse>> GetAllAsync();
    Task<GradeResponse?> GetByIdAsync(int id);

    // ✅ trả DTO đơn giản, không dùng SelectListItem
    Task<List<EnrollmentOptionResponse>> GetEnrollmentOptionsAsync();

    Task<int> CreateAsync(GradeUpsertRequest request);
    Task<bool> UpdateAsync(int id, GradeUpsertRequest request);
    Task<bool> DeleteAsync(int id);
    Task<List<CourseOptionResponse>> GetCourseOptionsAsync();
    Task<List<EnrollmentOptionResponse>> GetEnrollmentOptionsByCourseAsync(int courseId);

}
