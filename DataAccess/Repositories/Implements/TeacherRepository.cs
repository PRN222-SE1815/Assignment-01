using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly SchoolManagementDbContext _context;

        public TeacherRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        public async Task<List<Teacher>> GetAllTeachersAsync()
        {
            return await _context.Teachers
                .Include(t => t.User)
                .OrderBy(t => t.User.FullName)
                .ToListAsync();
        }

        public async Task<Teacher?> GetTeacherByIdAsync(int teacherId)
        {
            return await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);
        }
    }
}
