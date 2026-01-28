using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public sealed class ClassSectionRepository : IClassSectionRepository
{
    private readonly SchoolManagementDbContext _context;

    public ClassSectionRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ClassSection>> GetOpenSectionsAsync(int semesterId)
    {
        return await _context.ClassSections
            .AsNoTracking()
            .Include(cs => cs.Course)
            .Include(cs => cs.Teacher)
                .ThenInclude(t => t.TeacherNavigation)
            .Include(cs => cs.Semester)
            .Where(cs => cs.SemesterId == semesterId && cs.IsOpen)
            .OrderBy(cs => cs.SectionCode)
            .ToListAsync();
    }

    public Task<ClassSection?> GetSectionForRegistrationAsync(int classSectionId)
    {
        return _context.ClassSections
            .Include(cs => cs.Course)
            .Include(cs => cs.Semester)
            .SingleOrDefaultAsync(cs => cs.ClassSectionId == classSectionId);
    }

    public async Task<IReadOnlyList<ClassSection>> GetSectionsByTeacherAsync(int teacherId)
    {
        return await _context.ClassSections
            .AsNoTracking()
            .Include(cs => cs.Course)
            .Include(cs => cs.Semester)
            .Where(cs => cs.TeacherId == teacherId)
            .OrderBy(cs => cs.SectionCode)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ClassSection>> GetSectionsBySemesterAsync(int semesterId)
    {
        return await _context.ClassSections
            .AsNoTracking()
            .Include(cs => cs.Course)
            .Include(cs => cs.Teacher)
                .ThenInclude(t => t.TeacherNavigation)
            .Where(cs => cs.SemesterId == semesterId)
            .OrderBy(cs => cs.SectionCode)
            .ToListAsync();
    }

    public Task<ClassSection?> GetSectionDetailAsync(int classSectionId)
    {
        return _context.ClassSections
            .AsNoTracking()
            .Include(cs => cs.Course)
            .Include(cs => cs.Semester)
            .Include(cs => cs.Teacher)
                .ThenInclude(t => t.TeacherNavigation)
            .SingleOrDefaultAsync(cs => cs.ClassSectionId == classSectionId);
    }

    public Task<bool> IsTeacherAssignedAsync(int classSectionId, int teacherId)
    {
        return _context.ClassSections
            .AsNoTracking()
            .AnyAsync(cs => cs.ClassSectionId == classSectionId && cs.TeacherId == teacherId);
    }

    public Task<bool> IsTeacherAssignedToCourseAsync(int teacherId, int courseId)
    {
        return _context.ClassSections
            .AsNoTracking()
            .AnyAsync(cs => cs.CourseId == courseId && cs.TeacherId == teacherId);
    }
}
