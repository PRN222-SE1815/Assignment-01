using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public sealed class GradebookRepository : IGradebookRepository
{
    private readonly SchoolManagementDbContext _context;

    public GradebookRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public Task<GradeBook?> GetByIdAsync(int gradeBookId)
    {
        return _context.GradeBooks
            .Include(gb => gb.ClassSection)
                .ThenInclude(cs => cs.Course)
            .Include(gb => gb.ClassSection)
                .ThenInclude(cs => cs.Semester)
            .SingleOrDefaultAsync(gb => gb.GradeBookId == gradeBookId);
    }

    public Task<GradeBook?> GetByClassSectionIdAsync(int classSectionId)
    {
        return _context.GradeBooks
            .Include(gb => gb.ClassSection)
                .ThenInclude(cs => cs.Course)
            .Include(gb => gb.ClassSection)
                .ThenInclude(cs => cs.Semester)
            .SingleOrDefaultAsync(gb => gb.ClassSectionId == classSectionId);
    }

    public async Task<IReadOnlyList<GradeBook>> GetByClassSectionIdsAsync(IReadOnlyCollection<int> classSectionIds)
    {
        return await _context.GradeBooks
            .AsNoTracking()
            .Include(gb => gb.ClassSection)
                .ThenInclude(cs => cs.Course)
            .Include(gb => gb.ClassSection)
                .ThenInclude(cs => cs.Semester)
            .Where(gb => classSectionIds.Contains(gb.ClassSectionId))
            .ToListAsync();
    }

    public async Task AddAsync(GradeBook gradeBook)
    {
        _context.GradeBooks.Add(gradeBook);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(GradeBook gradeBook)
    {
        _context.GradeBooks.Update(gradeBook);
        await _context.SaveChangesAsync();
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await action();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
