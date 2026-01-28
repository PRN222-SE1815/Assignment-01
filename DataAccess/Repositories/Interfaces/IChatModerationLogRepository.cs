using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IChatModerationLogRepository
{
    Task<ChatModerationLog> InsertModerationLogAsync(int roomId, int actorUserId, string action, int? targetUserId, long? targetMessageId, string? metadataJson, DateTime createdAt);
}
