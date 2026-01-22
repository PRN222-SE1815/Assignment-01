using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly SchoolManagementDbContext _context;

        public EnrollmentRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        public async Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                        .ThenInclude(t => t.User)
                .Where(e => e.StudentId == studentId)
                .OrderByDescending(e => e.EnrollDate)
                .ToListAsync();
        }

        public async Task<Enrollment?> GetEnrollmentByStudentAndCourseAsync(int studentId, int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        }

        public async Task<Enrollment> CreateEnrollmentAsync(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            return enrollment;
        }

        public async Task<bool> UpdateEnrollmentStatusAsync(int enrollmentId, string status)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null)
                return false;

            enrollment.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsStudentEnrolledAsync(int studentId, int courseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && 
                              e.CourseId == courseId && 
                              e.Status == "Active");
        }

        public async Task<int> GetEnrolledCountByCourseAsync(int courseId)
        {
            return await _context.Enrollments
                .CountAsync(e => e.CourseId == courseId && e.Status == "Active");
        }
    }
}
