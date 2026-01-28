using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IClassSectionRepository
{
    Task<IReadOnlyList<ClassSection>> GetOpenSectionsAsync(int semesterId);
    Task<ClassSection?> GetSectionForRegistrationAsync(int classSectionId);
    Task<IReadOnlyList<ClassSection>> GetSectionsByTeacherAsync(int teacherId);
    Task<IReadOnlyList<ClassSection>> GetSectionsBySemesterAsync(int semesterId);
    Task<ClassSection?> GetSectionDetailAsync(int classSectionId);
    Task<bool> IsTeacherAssignedAsync(int classSectionId, int teacherId);
    Task<bool> IsTeacherAssignedToCourseAsync(int teacherId, int courseId);
}
