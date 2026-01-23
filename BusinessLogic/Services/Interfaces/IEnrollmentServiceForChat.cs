using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;

namespace BusinessLogic.Services.Interfaces
{
    /// <summary>
    /// Service for managing student enrollments
    /// </summary>
    public interface IEnrollmentServiceForChat
    {
        /// <summary>
        /// Enroll student to course and auto-create course conversation
        /// </summary>
        Task<EnrollmentResponseForChat> EnrollStudentToCourseAsync(int studentId, int courseId);
        
        /// <summary>
        /// Get all enrollments for a student
        /// </summary>
        Task<List<EnrollmentResponseForChat>> GetStudentEnrollmentsAsync(int studentId);
        
        /// <summary>
        /// Get all students enrolled in a course
        /// </summary>
        Task<List<EnrollmentResponseForChat>> GetCourseEnrollmentsAsync(int courseId);
    }
}

