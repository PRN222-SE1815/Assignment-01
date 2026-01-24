using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces
{
    public interface ICourseScheduleRepository
    {
        Task<List<CourseSchedule>> GetSchedulesByStudentUserIdAsync(int studentUserId);
        Task<List<CourseSchedule>> GetSchedulesByCourseIdsAsync(IEnumerable<int> courseIds);
    }
}

