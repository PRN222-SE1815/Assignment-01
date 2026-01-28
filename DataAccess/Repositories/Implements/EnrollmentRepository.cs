using System.Data;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public sealed class EnrollmentRepository : IEnrollmentRepository
{
    private readonly SchoolManagementDbContext _context;

    public EnrollmentRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Enrollment>> GetStudentEnrollmentsAsync(int studentId, int semesterId, IReadOnlyCollection<string> statuses)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.ClassSection)
                .ThenInclude(cs => cs.Teacher)
                    .ThenInclude(t => t.TeacherNavigation)
            .Include(e => e.Course)
            .Include(e => e.Semester)
            .Where(e => e.StudentId == studentId && e.SemesterId == semesterId && statuses.Contains(e.Status))
            .OrderBy(e => e.ClassSectionId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Enrollment>> GetStudentEnrollmentsForGradesAsync(int studentId, IReadOnlyCollection<string> statuses)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.ClassSection)
                .ThenInclude(cs => cs.Course)
            .Include(e => e.ClassSection)
                .ThenInclude(cs => cs.Semester)
            .Where(e => e.StudentId == studentId && statuses.Contains(e.Status))
            .OrderByDescending(e => e.SemesterId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<int>> GetStudentEnrolledSectionIdsAsync(int studentId, int semesterId, IReadOnlyCollection<string> statuses)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId && e.SemesterId == semesterId && statuses.Contains(e.Status))
            .Select(e => e.ClassSectionId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<int>> GetEnrolledUserIdsForSectionAsync(int classSectionId, IReadOnlyCollection<string> statuses)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.ClassSectionId == classSectionId && statuses.Contains(e.Status))
            .Select(e => e.StudentId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Enrollment>> GetRosterBySectionAsync(int classSectionId, IReadOnlyCollection<string> statuses)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
                .ThenInclude(s => s.StudentNavigation)
            .Where(e => e.ClassSectionId == classSectionId && statuses.Contains(e.Status))
            .OrderBy(e => e.Student.StudentNavigation.FullName)
            .ToListAsync();
    }

    public Task<Enrollment?> GetEnrollmentByIdAsync(int enrollmentId)
    {
        return _context.Enrollments
            .Include(e => e.ClassSection)
            .Include(e => e.Semester)
            .SingleOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
    }

    public Task<Enrollment?> GetEnrollmentBySectionAsync(int studentId, int classSectionId)
    {
        return _context.Enrollments
            .Include(e => e.ClassSection)
            .Include(e => e.Semester)
            .SingleOrDefaultAsync(e => e.StudentId == studentId && e.ClassSectionId == classSectionId);
    }

    public Task<bool> IsStudentEnrolledInCourseAsync(int studentId, int courseId, IReadOnlyCollection<string> statuses)
    {
        return _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId && statuses.Contains(e.Status));
    }

    public Task<bool> ExistsEnrollmentAsync(int studentId, int courseId, int semesterId, IReadOnlyCollection<string> statuses)
    {
        return _context.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId && e.SemesterId == semesterId && statuses.Contains(e.Status));
    }

    public async Task<int> GetTotalCreditsAsync(int studentId, int semesterId, IReadOnlyCollection<string> statuses)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId && e.SemesterId == semesterId && statuses.Contains(e.Status))
            .SumAsync(e => e.CreditsSnapshot);
    }

    public async Task<bool> RegisterEnrollmentAsync(Enrollment enrollment, bool incrementCapacity)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            if (incrementCapacity)
            {
                var rows = await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE ClassSections SET CurrentEnrollment = CurrentEnrollment + 1 WHERE ClassSectionId = {enrollment.ClassSectionId} AND CurrentEnrollment < MaxCapacity");

                if (rows == 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateEnrollmentStatusAsync(int enrollmentId, string status, bool decrementCapacity)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var enrollment = await _context.Enrollments.SingleOrDefaultAsync(e => e.EnrollmentId == enrollmentId);
            if (enrollment == null)
            {
                throw new InvalidOperationException("Enrollment not found.");
            }

            enrollment.Status = status;
            enrollment.UpdatedAt = DateTime.UtcNow;

            if (decrementCapacity)
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE ClassSections SET CurrentEnrollment = CASE WHEN CurrentEnrollment > 0 THEN CurrentEnrollment - 1 ELSE 0 END WHERE ClassSectionId = {enrollment.ClassSectionId}");
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
