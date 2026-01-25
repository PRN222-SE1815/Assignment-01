using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces
{
    public interface ITeacherRepository
    {
        Task<List<Teacher>> GetAllTeachersAsync();
        Task<Teacher?> GetTeacherByIdAsync(int teacherId);
    }
}
