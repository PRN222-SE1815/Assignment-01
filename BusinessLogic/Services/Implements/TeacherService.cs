using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements;

public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _teacherRepository;

    public TeacherService(ITeacherRepository teacherRepository)
    {
        _teacherRepository = teacherRepository;
    }

    public Task<List<Teacher>> GetAllTeachersAsync()
        => _teacherRepository.GetAllTeachersAsync();

    public Task<Teacher?> GetTeacherByIdAsync(int teacherId)
        => _teacherRepository.GetTeacherByIdAsync(teacherId);
}
