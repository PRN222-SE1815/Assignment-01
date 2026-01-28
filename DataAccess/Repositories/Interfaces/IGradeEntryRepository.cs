using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IGradeEntryRepository
{
    Task<IReadOnlyList<GradeEntry>> GetByGradeBookIdAsync(int gradeBookId);
    Task AddRangeAsync(IEnumerable<GradeEntry> entries);
    Task UpdateRangeAsync(IEnumerable<GradeEntry> entries);
}
