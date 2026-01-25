using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;

    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public Task<List<Student>> GetAllStudentsAsync()
        => _studentRepository.GetAllStudentsAsync();

    public Task<Student?> GetStudentByIdAsync(int studentId)
        => _studentRepository.GetStudentByIdAsync(studentId);

    public Task<Student?> GetStudentByUserIdAsync(int userId)
        => _studentRepository.GetStudentByUserIdAsync(userId);
}
