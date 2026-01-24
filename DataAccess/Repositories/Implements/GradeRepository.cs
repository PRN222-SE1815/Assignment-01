using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public class GradeRepository : IGradeRepository
{
    private readonly SchoolManagementDbContext _db;

    public GradeRepository(SchoolManagementDbContext db)
    {
        _db = db;
    }

    public Task<List<Grade>> GetAllAsync()
    {
        return _db.Grades
            .Include(g => g.Enrollment)
                .ThenInclude(e => e.Student)
                    .ThenInclude(s => s.User)
            .Include(g => g.Enrollment)
                .ThenInclude(e => e.Course)
            .ToListAsync();
    }

    public Task<Grade?> GetByIdAsync(int id)
    {
        return _db.Grades
            .Include(g => g.Enrollment)
                .ThenInclude(e => e.Student)
                    .ThenInclude(s => s.User)
            .Include(g => g.Enrollment)
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(g => g.GradeId == id);
    }

    public Task<List<Enrollment>> GetEnrollmentsAsync()
    {
        return _db.Enrollments
            .Include(e => e.Student).ThenInclude(s => s.User)
            .Include(e => e.Course)
            .ToListAsync();
    }

    public async Task CreateAsync(Grade entity)
    {
        _db.Grades.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Grade entity)
    {
        _db.Grades.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Grade entity)
    {
        _db.Grades.Remove(entity);
        await _db.SaveChangesAsync();
    }
    public async Task<List<Course>> GetCoursesAsync()
    {
        return await _db.Courses
            .AsNoTracking()
            .OrderBy(c => c.CourseName)
            .ToListAsync();
    }

    public async Task<List<Enrollment>> GetEnrollmentsByCourseAsync(int courseId)
    {
        return await _db.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Include(e => e.Student)
                .ThenInclude(s => s.User)
            .OrderBy(e => e.Student.User.FullName)
            .ToListAsync();
    }

}
