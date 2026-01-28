using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public sealed class GradeItemRepository : IGradeItemRepository
{
    private readonly SchoolManagementDbContext _context;

    public GradeItemRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GradeItem>> GetByGradeBookIdAsync(int gradeBookId)
    {
        return await _context.GradeItems
            .AsNoTracking()
            .Where(item => item.GradeBookId == gradeBookId)
            .OrderBy(item => item.SortOrder)
            .ToListAsync();
    }

    public Task<GradeItem?> GetByIdAsync(int gradeItemId)
    {
        return _context.GradeItems
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.GradeItemId == gradeItemId);
    }

    public async Task AddRangeAsync(IEnumerable<GradeItem> items)
    {
        _context.GradeItems.AddRange(items);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<GradeItem> items)
    {
        _context.GradeItems.UpdateRange(items);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRangeAsync(IEnumerable<GradeItem> items)
    {
        _context.GradeItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}
