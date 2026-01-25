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

        public async Task<List<int>> GetCourseIdsByTeacherUserIdAsync(int teacherUserId)
        {
            return await _context.Courses
                .Where(c => c.Teacher.UserId == teacherUserId)
                .Select(c => c.CourseId)
                .ToListAsync();
        }

        public async Task<List<Course>> GetUserCoursesAsync(int userId)
        {
            // Get courses where user is either teacher or enrolled student
            var teacherCourses = await _context.Courses
                .Include(c => c.Teacher)
                .Where(c => c.Teacher != null && c.Teacher.UserId == userId)
                .ToListAsync();

            var studentCourses = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student) // Include Student to access UserId
                    .ThenInclude(s => s.User) // Include User from Student
                .Where(e => e.Student.UserId == userId && e.Status == "Active")
                .Select(e => e.Course)
                .ToListAsync();

            // Combine and return distinct courses
            var allCourses = teacherCourses.Concat(studentCourses)
                .DistinctBy(c => c.CourseId)
                .OrderBy(c => c.CourseName)
                .ToList();

            return allCourses;
        }

        public async Task<List<int>> GetCourseParticipantUserIdsAsync(int courseId)
        {
            var participantIds = new List<int>();

            // Get teacher's user ID
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course?.Teacher?.UserId > 0)
            {
                participantIds.Add(course.Teacher.UserId);
            }

            // Get enrolled students' user IDs
            var studentIds = await GetEnrolledStudentUserIdsAsync(courseId);
            participantIds.AddRange(studentIds);

            return participantIds.Distinct().ToList();
        }

        // CRUD methods implementation
        public async Task<Course> CreateCourseAsync(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<Course> UpdateCourseAsync(Course course)
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<bool> DeleteCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return false;

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CourseCodeExistsAsync(string courseCode, int? excludeCourseId = null)
        {
            var query = _context.Courses.Where(c => c.CourseCode == courseCode);
            
            if (excludeCourseId.HasValue)
            {
                query = query.Where(c => c.CourseId != excludeCourseId.Value);
            }

            return await query.AnyAsync();
        }
    }
}