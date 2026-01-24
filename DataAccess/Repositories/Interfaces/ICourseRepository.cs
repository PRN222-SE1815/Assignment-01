using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces
{
    public interface ICourseRepository
    {
        Task<Course?> GetCourseByIdAsync(int courseId);
        Task<Course?> GetCourseWithTeacherAsync(int courseId);
        Task<List<int>> GetEnrolledStudentUserIdsAsync(int courseId);
        Task<List<int>> GetCourseIdsByTeacherUserIdAsync(int teacherUserId);
    }
}
