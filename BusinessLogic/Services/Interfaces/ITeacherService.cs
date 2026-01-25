using DataAccess.Entities;

namespace BusinessLogic.Services.Interfaces;

public interface ITeacherService
{
    Task<List<Teacher>> GetAllTeachersAsync();
    Task<Teacher?> GetTeacherByIdAsync(int teacherId);
}
