using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements
{
    public class CourseScheduleRepository : ICourseScheduleRepository
    {
        private readonly SchoolManagementDbContext _context;

        public CourseScheduleRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseSchedule>> GetSchedulesByStudentUserIdAsync(int studentUserId)
        {
            // Get student's enrolled courses and their schedules
            return await _context.Enrollments
                .Where(e => e.Student.UserId == studentUserId && e.Status == "Active")
                .Select(e => e.Course)
                .SelectMany(c => c.CourseSchedules)
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .ToListAsync();
        }

        public async Task<List<CourseSchedule>> GetSchedulesByCourseIdsAsync(IEnumerable<int> courseIds)
        {
            return await _context.CourseSchedules
                .Where(cs => courseIds.Contains(cs.CourseId))
                .Include(cs => cs.Course)
                    .ThenInclude(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .ToListAsync();
        }
    }
}

