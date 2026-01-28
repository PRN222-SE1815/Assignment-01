using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IGradeItemRepository
{
    Task<IReadOnlyList<GradeItem>> GetByGradeBookIdAsync(int gradeBookId);
    Task<GradeItem?> GetByIdAsync(int gradeItemId);
    Task AddRangeAsync(IEnumerable<GradeItem> items);
    Task UpdateRangeAsync(IEnumerable<GradeItem> items);
    Task DeleteRangeAsync(IEnumerable<GradeItem> items);
}
