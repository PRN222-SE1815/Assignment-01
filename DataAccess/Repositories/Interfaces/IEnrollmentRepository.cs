using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces
{
    public interface IEnrollmentRepository
    {
        Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId);
        Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId);
        Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId);
        Task<Enrollment?> GetEnrollmentByStudentAndCourseAsync(int studentId, int courseId);
        Task<Enrollment> CreateEnrollmentAsync(Enrollment enrollment);
        Task<bool> UpdateEnrollmentStatusAsync(int enrollmentId, string status);
        Task<bool> IsStudentEnrolledAsync(int studentId, int courseId);
        Task<int> GetEnrolledCountByCourseAsync(int courseId);
    }
}
