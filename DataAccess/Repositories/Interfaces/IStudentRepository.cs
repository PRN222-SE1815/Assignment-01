using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces
{
    /// <summary>
    /// Repository for managing students
    /// </summary>
    public interface IStudentRepository
    {
        /// <summary>
        /// Get student by UserId
        /// </summary>
        Task<Student?> GetStudentByUserIdAsync(int userId);
        
        /// <summary>
        /// Get student by StudentId
        /// </summary>
        Task<Student?> GetStudentByIdAsync(int studentId);
    }
}
