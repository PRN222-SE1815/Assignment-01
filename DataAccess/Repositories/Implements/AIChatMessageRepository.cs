using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public sealed class AIChatMessageRepository : IAIChatMessageRepository
{
    private readonly SchoolManagementDbContext _context;

    public AIChatMessageRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task<long> AddMessageAsync(long sessionId, string senderType, string content, DateTime createdAt)
    {
        var message = new AIChatMessage
        {
            ChatSessionId = sessionId,
            SenderType = senderType,
            Content = content,
            CreatedAt = createdAt
        };

        _context.AIChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message.ChatMessageId;
    }

    public async Task<IReadOnlyList<AIChatMessage>> ListRecentMessagesAsync(long sessionId, int limit)
    {
        var resolvedLimit = limit <= 0 ? 20 : limit;
        return await _context.AIChatMessages
            .AsNoTracking()
            .Where(m => m.ChatSessionId == sessionId)
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.ChatMessageId)
            .Take(resolvedLimit)
            .ToListAsync();
    }
}
