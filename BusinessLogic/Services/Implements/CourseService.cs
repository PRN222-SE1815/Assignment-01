using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUserRepository _userRepository;

    public CourseService(ICourseRepository courseRepository, IUserRepository userRepository)
    {
        _courseRepository = courseRepository;
        _userRepository = userRepository;
    }

    public async Task<List<CourseBasicResponse>> GetUserCoursesAsync(int userId)
    {
        var courses = await _courseRepository.GetUserCoursesAsync(userId);

        return courses.Select(c => new CourseBasicResponse
        {
            CourseId = c.CourseId,
            CourseCode = c.CourseCode,
            CourseName = c.CourseName,
            Semester = c.Semester
        }).ToList();
    }

    public async Task<List<UserSearchResponse>> SearchCourseParticipantsAsync(int courseId, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<UserSearchResponse>();
        }

        // Get all participant user IDs in this course
        var participantIds = await _courseRepository.GetCourseParticipantUserIdsAsync(courseId);

        if (!participantIds.Any())
        {
            return new List<UserSearchResponse>();
        }

        // Search users only within course participants
        var allUsers = await _userRepository.SearchUsersAsync(searchTerm);

        var courseParticipants = allUsers
            .Where(u => participantIds.Contains(u.UserId))
            .Select(u => new UserSearchResponse
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Username = u.Username,
                RoleName = u.Role?.RoleName ?? "Unknown"
            })
            .ToList();

        return courseParticipants;
    }

    public async Task<List<int>> GetCourseParticipantUserIdsAsync(int courseId)
    {
        return await _courseRepository.GetCourseParticipantUserIdsAsync(courseId);
    }
}

