using DataAccess.Entities;

namespace BusinessLogic.Services.Interfaces;

public interface IStudentService
{
    Task<List<Student>> GetAllStudentsAsync();
    Task<Student?> GetStudentByIdAsync(int studentId);
    Task<Student?> GetStudentByUserIdAsync(int userId);
}
