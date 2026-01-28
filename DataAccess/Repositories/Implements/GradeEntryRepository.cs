using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public sealed class GradeEntryRepository : IGradeEntryRepository
{
    private readonly SchoolManagementDbContext _context;

    public GradeEntryRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GradeEntry>> GetByGradeBookIdAsync(int gradeBookId)
    {
        return await _context.GradeEntries
            .AsNoTracking()
            .Where(entry => entry.GradeItem.GradeBookId == gradeBookId)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<GradeEntry> entries)
    {
        _context.GradeEntries.AddRange(entries);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<GradeEntry> entries)
    {
        _context.GradeEntries.UpdateRange(entries);
        await _context.SaveChangesAsync();
    }
}
