using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IEnrollmentRepository
{
    Task<IReadOnlyList<Enrollment>> GetStudentEnrollmentsAsync(int studentId, int semesterId, IReadOnlyCollection<string> statuses);
    Task<IReadOnlyList<Enrollment>> GetStudentEnrollmentsForGradesAsync(int studentId, IReadOnlyCollection<string> statuses);
    Task<IReadOnlyList<int>> GetStudentEnrolledSectionIdsAsync(int studentId, int semesterId, IReadOnlyCollection<string> statuses);
    Task<IReadOnlyList<int>> GetEnrolledUserIdsForSectionAsync(int classSectionId, IReadOnlyCollection<string> statuses);
    Task<IReadOnlyList<Enrollment>> GetRosterBySectionAsync(int classSectionId, IReadOnlyCollection<string> statuses);
    Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId);
    Task<Enrollment?> GetEnrollmentBySectionAsync(int studentId, int classSectionId);
    Task<bool> IsStudentEnrolledInCourseAsync(int studentId, int courseId, IReadOnlyCollection<string> statuses);
    Task<bool> ExistsEnrollmentAsync(int studentId, int courseId, int semesterId, IReadOnlyCollection<string> statuses);
    Task<int> GetTotalCreditsAsync(int studentId, int semesterId, IReadOnlyCollection<string> statuses);
    Task<bool> RegisterEnrollmentAsync(Enrollment enrollment, bool incrementCapacity);
    Task UpdateEnrollmentStatusAsync(int enrollmentId, string status, bool decrementCapacity);
}
