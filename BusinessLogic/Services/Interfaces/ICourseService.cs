using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;

namespace BusinessLogic.Services.Interfaces;

public interface ICourseService
{
    Task<List<CourseBasicResponse>> GetUserCoursesAsync(int userId);
    Task<List<UserSearchResponse>> SearchCourseParticipantsAsync(int courseId, string searchTerm);
    Task<List<int>> GetCourseParticipantUserIdsAsync(int courseId);
    
    // Admin CRUD operations
    Task<List<CourseDetailResponse>> GetAllCoursesAsync();
    Task<CourseDetailResponse?> GetCourseByIdAsync(int courseId);
    Task<CourseDetailResponse> CreateCourseAsync(CreateCourseRequest request);
    Task<CourseDetailResponse> UpdateCourseAsync(UpdateCourseRequest request);
    Task<bool> DeleteCourseAsync(int courseId);
    Task<bool> CourseCodeExistsAsync(string courseCode, int? excludeCourseId = null);
}
