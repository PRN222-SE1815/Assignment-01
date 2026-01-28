using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public sealed class SemesterRepository : ISemesterRepository
{
    private readonly SchoolManagementDbContext _context;

    public SemesterRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public Task<Semester?> GetActiveSemesterAsync()
    {
        return _context.Semesters
            .AsNoTracking()
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync(s => s.IsActive);
    }

    public Task<Semester?> GetSemesterAsync(int semesterId)
    {
        return _context.Semesters.AsNoTracking().SingleOrDefaultAsync(s => s.SemesterId == semesterId);
    }
}
