using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces
{
    /// <summary>
    /// Repository for managing enrollments
    /// </summary>
    public interface IEnrollmentRepository
    {
        /// <summary>
        /// Create a new enrollment
        /// </summary>
        Task<Enrollment> CreateEnrollmentAsync(Enrollment enrollment);
        
        /// <summary>
        /// Get enrollment by ID
        /// </summary>
        Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId);
        
        /// <summary>
        /// Get all enrollments for a student
        /// </summary>
        Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId);
        
        /// <summary>
        /// Get all enrollments for a course
        /// </summary>
        Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId);
        
        /// <summary>
        /// Check if student is already enrolled in course
        /// </summary>
        Task<bool> IsStudentEnrolledAsync(int studentId, int courseId);
    }
}

