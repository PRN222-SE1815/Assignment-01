using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IGradebookRepository
{
    Task<GradeBook?> GetByIdAsync(int gradeBookId);
    Task<GradeBook?> GetByClassSectionIdAsync(int classSectionId);
    Task<IReadOnlyList<GradeBook>> GetByClassSectionIdsAsync(IReadOnlyCollection<int> classSectionIds);
    Task AddAsync(GradeBook gradeBook);
    Task UpdateAsync(GradeBook gradeBook);
    Task ExecuteInTransactionAsync(Func<Task> action);
}
