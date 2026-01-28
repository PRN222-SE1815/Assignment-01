using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IAIChatMessageRepository
{
    Task<long> AddMessageAsync(long sessionId, string senderType, string content, DateTime createdAt);
    Task<IReadOnlyList<AIChatMessage>> ListRecentMessagesAsync(long sessionId, int limit);
}
