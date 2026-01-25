using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public CourseService(
        ICourseRepository courseRepository, 
        IUserRepository userRepository,
        IEnrollmentRepository enrollmentRepository)
    {
        _courseRepository = courseRepository;
        _userRepository = userRepository;
        _enrollmentRepository = enrollmentRepository;
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

    // Admin CRUD operations implementation
    public async Task<List<CourseDetailResponse>> GetAllCoursesAsync()
    {
        var courses = await _courseRepository.GetAllCoursesAsync();
        var result = new List<CourseDetailResponse>();

        foreach (var course in courses)
        {
            var enrolledCount = await _enrollmentRepository.GetEnrolledCountByCourseAsync(course.CourseId);
            
            result.Add(new CourseDetailResponse
            {
                CourseId = course.CourseId,
                CourseCode = course.CourseCode,
                CourseName = course.CourseName,
                Credits = course.Credits,
                Semester = course.Semester,
                TeacherId = course.TeacherId,
                TeacherName = course.Teacher?.User?.FullName,
                Department = course.Teacher?.Department,
                EnrolledCount = enrolledCount
            });
        }

        return result;
    }

    public async Task<CourseDetailResponse?> GetCourseByIdAsync(int courseId)
    {
        var course = await _courseRepository.GetCourseWithTeacherAsync(courseId);
        if (course == null) return null;

        var enrolledCount = await _enrollmentRepository.GetEnrolledCountByCourseAsync(courseId);

        return new CourseDetailResponse
        {
            CourseId = course.CourseId,
            CourseCode = course.CourseCode,
            CourseName = course.CourseName,
            Credits = course.Credits,
            Semester = course.Semester,
            TeacherId = course.TeacherId,
            TeacherName = course.Teacher?.User?.FullName,
            Department = course.Teacher?.Department,
            EnrolledCount = enrolledCount
        };
    }

    public async Task<CourseDetailResponse> CreateCourseAsync(CreateCourseRequest request)
    {
        // Check if course code already exists
        if (await _courseRepository.CourseCodeExistsAsync(request.CourseCode))
        {
            throw new InvalidOperationException($"Course code '{request.CourseCode}' already exists!");
        }

        var course = new Course
        {
            CourseCode = request.CourseCode,
            CourseName = request.CourseName,
            Credits = request.Credits,
            Semester = request.Semester,
            TeacherId = request.TeacherId
        };

        var createdCourse = await _courseRepository.CreateCourseAsync(course);

        // Reload with teacher info
        var result = await GetCourseByIdAsync(createdCourse.CourseId);
        return result!;
    }

    public async Task<CourseDetailResponse> UpdateCourseAsync(UpdateCourseRequest request)
    {
        var course = await _courseRepository.GetCourseByIdAsync(request.CourseId);
        if (course == null)
        {
            throw new KeyNotFoundException($"Course with ID {request.CourseId} not found!");
        }

        // Check if course code is being changed and already exists
        if (await _courseRepository.CourseCodeExistsAsync(request.CourseCode, request.CourseId))
        {
            throw new InvalidOperationException($"Course code '{request.CourseCode}' already exists!");
        }

        course.CourseCode = request.CourseCode;
        course.CourseName = request.CourseName;
        course.Credits = request.Credits;
        course.Semester = request.Semester;
        course.TeacherId = request.TeacherId;

        await _courseRepository.UpdateCourseAsync(course);

        var result = await GetCourseByIdAsync(course.CourseId);
        return result!;
    }

    public async Task<bool> DeleteCourseAsync(int courseId)
    {
        var course = await _courseRepository.GetCourseByIdAsync(courseId);
        if (course == null)
        {
            return false;
        }

        // Check if course has enrollments
        var enrolledCount = await _enrollmentRepository.GetEnrolledCountByCourseAsync(courseId);
        if (enrolledCount > 0)
        {
            throw new InvalidOperationException("Cannot delete course with active enrollments!");
        }

        return await _courseRepository.DeleteCourseAsync(courseId);
    }

    public async Task<bool> CourseCodeExistsAsync(string courseCode, int? excludeCourseId = null)
    {
        return await _courseRepository.CourseCodeExistsAsync(courseCode, excludeCourseId);
    }
}

