using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public sealed class PrerequisiteRepository : IPrerequisiteRepository
{
    private readonly SchoolManagementDbContext _context;

    public PrerequisiteRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<int>> GetPrerequisiteCourseIdsAsync(int courseId)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.CourseId == courseId)
            .SelectMany(c => c.Courses.Select(p => p.CourseId))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<int>> GetPassedCourseIdsAsync(int studentId)
    {
        var completedEnrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.GradeEntries)
            .Where(e => e.StudentId == studentId && e.Status == "COMPLETED")
            .ToListAsync();

        var passedCourseIds = new List<int>();

        foreach (var enrollment in completedEnrollments)
        {
            var scores = enrollment.GradeEntries
                .Where(ge => ge.Score.HasValue)
                .Select(ge => ge.Score!.Value)
                .ToList();

            if (scores.Count == 0)
            {
                continue;
            }

            var averageScore = scores.Average();
            if (averageScore >= 5m)
            {
                passedCourseIds.Add(enrollment.CourseId);
            }
        }

        return passedCourseIds;
    }
}
