using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces
{
    public interface ICourseRepository
    {
        Task<List<Course>> GetAllCoursesAsync();
        Task<Course?> GetCourseByIdAsync(int courseId);
        Task<Course?> GetCourseWithTeacherAsync(int courseId);
        Task<List<int>> GetEnrolledStudentUserIdsAsync(int courseId);
        Task<List<int>> GetCourseIdsByTeacherUserIdAsync(int teacherUserId);
        Task<List<Course>> GetUserCoursesAsync(int userId);
        Task<List<int>> GetCourseParticipantUserIdsAsync(int courseId);
    }
}