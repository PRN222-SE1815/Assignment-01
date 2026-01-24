using BusinessLogic.DTOs.Responses;

namespace BusinessLogic.Services.Interfaces;

public interface ICourseService
{
    Task<List<CourseBasicResponse>> GetUserCoursesAsync(int userId);
    Task<List<UserSearchResponse>> SearchCourseParticipantsAsync(int courseId, string searchTerm);
    Task<List<int>> GetCourseParticipantUserIdsAsync(int courseId);
}
