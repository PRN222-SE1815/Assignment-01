using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace DataAccess.Repositories.Implements;

public sealed class ChatModerationLogRepository : IChatModerationLogRepository
{
    private readonly SchoolManagementDbContext _context;

    public ChatModerationLogRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task<ChatModerationLog> InsertModerationLogAsync(int roomId, int actorUserId, string action, int? targetUserId, long? targetMessageId, string? metadataJson, DateTime createdAt)
    {
        var log = new ChatModerationLog
        {
            RoomId = roomId,
            ActorUserId = actorUserId,
            Action = action,
            TargetUserId = targetUserId,
            TargetMessageId = targetMessageId,
            MetadataJson = metadataJson,
            CreatedAt = createdAt
        };

        _context.ChatModerationLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }
}
