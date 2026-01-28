using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface ISemesterRepository
{
    Task<Semester?> GetActiveSemesterAsync();
    Task<Semester?> GetSemesterAsync(int semesterId);
}
