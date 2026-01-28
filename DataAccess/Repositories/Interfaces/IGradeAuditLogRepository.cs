using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IGradeAuditLogRepository
{
    Task AddRangeAsync(IEnumerable<GradeAuditLog> logs);
}
