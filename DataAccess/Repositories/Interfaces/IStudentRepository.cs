using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        Task<List<Student>> GetAllStudentsAsync();
        Task<Student?> GetStudentByIdAsync(int studentId);
        Task<Student?> GetStudentByUserIdAsync(int userId);
    }
}
