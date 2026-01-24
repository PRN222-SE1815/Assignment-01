using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces
{
    public interface ICourseRepository
    {
        Task<Course?> GetCourseByIdAsync(int courseId);
        Task<Course?> GetCourseWithTeacherAsync(int courseId);
        Task<List<int>> GetEnrolledStudentUserIdsAsync(int courseId);
        Task<List<int>> GetCourseIdsByTeacherUserIdAsync(int teacherUserId);
        Task<List<Course>> GetUserCoursesAsync(int userId); // NEW: Get courses where user is teacher or student
        Task<List<int>> GetCourseParticipantUserIdsAsync(int courseId); // NEW: Get all participant user IDs (teacher + students)
    }
}
