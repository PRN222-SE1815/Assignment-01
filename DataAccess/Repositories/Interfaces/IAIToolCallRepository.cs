namespace DataAccess.Repositories.Interfaces;

public interface IAIToolCallRepository
{
    Task<long> AddToolCallAsync(long sessionId, string toolName, string requestJson, string? responseJson, string status, DateTime createdAt);
}
