using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements
{
    public class CourseRepository : ICourseRepository
    {
        private readonly SchoolManagementDbContext _context;

        public CourseRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .OrderBy(c => c.CourseCode)
                .ToListAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(int courseId)
        {
            return await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<Course?> GetCourseWithTeacherAsync(int courseId)
        {
            return await _context.Courses
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<List<int>> GetEnrolledStudentUserIdsAsync(int courseId)
        {
            return await _context.Enrollments
                .Where(e => e.CourseId == courseId && e.Status == "Active")
                .Select(e => e.Student.UserId)
                .ToListAsync();
        }
    }
}