using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IAIChatSessionRepository
{
    Task<long> CreateSessionAsync(int userId, string purpose, string? modelName, string? promptVersion, DateTime createdAt);
    Task<AIChatSession?> GetByIdAsync(long sessionId);
    Task CompleteSessionAsync(long sessionId, string state, DateTime completedAt);
}
