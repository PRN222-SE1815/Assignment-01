using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IGradeRepository
{
    Task<List<Grade>> GetAllAsync();
    Task<Grade?> GetByIdAsync(int id);

    Task<List<Enrollment>> GetEnrollmentsAsync();

    Task CreateAsync(Grade entity);
    Task UpdateAsync(Grade entity);
    Task DeleteAsync(Grade entity);
    Task<List<Course>> GetCoursesAsync();
    Task<List<Enrollment>> GetEnrollmentsByCourseAsync(int courseId);

}
