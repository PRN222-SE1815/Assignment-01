using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace DataAccess.Repositories.Implements;

public sealed class GradeAuditLogRepository : IGradeAuditLogRepository
{
    private readonly SchoolManagementDbContext _context;

    public GradeAuditLogRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<GradeAuditLog> logs)
    {
        _context.GradeAuditLogs.AddRange(logs);
        await _context.SaveChangesAsync();
    }
}
