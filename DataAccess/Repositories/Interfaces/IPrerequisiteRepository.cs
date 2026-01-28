namespace DataAccess.Repositories.Interfaces;

public interface IPrerequisiteRepository
{
    Task<IReadOnlyList<int>> GetPrerequisiteCourseIdsAsync(int courseId);
    Task<IReadOnlyList<int>> GetPassedCourseIdsAsync(int studentId);
}
