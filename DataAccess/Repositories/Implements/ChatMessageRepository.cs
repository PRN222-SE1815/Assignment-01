using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DataAccess.Repositories.Implements;

public sealed class ChatMessageRepository : IChatMessageRepository
{
    private readonly SchoolManagementDbContext _context;

    public ChatMessageRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task<long> InsertMessageAsync(int roomId, int senderId, string messageType, string? content, DateTime createdAt)
    {
        var message = new ChatMessage
        {
            RoomId = roomId,
            SenderId = senderId,
            MessageType = messageType,
            Content = content,
            CreatedAt = createdAt
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message.MessageId;
    }

    public async Task<long> InsertMessageWithAttachmentsAsync(int roomId, int senderId, string messageType, string? content, DateTime createdAt, IReadOnlyList<ChatMessageAttachment> attachments)
    {
        var message = new ChatMessage
        {
            RoomId = roomId,
            SenderId = senderId,
            MessageType = messageType,
            Content = content,
            CreatedAt = createdAt,
            ChatMessageAttachments = attachments.ToList()
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message.MessageId;
    }

    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(int roomId, long? beforeMessageId, int pageSize)
    {
        var query = _context.ChatMessages
            .AsNoTracking()
            .Where(m => m.RoomId == roomId);

        if (beforeMessageId.HasValue)
        {
            var reference = await _context.ChatMessages
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.MessageId == beforeMessageId.Value);

            if (reference != null)
            {
                query = query.Where(m => m.CreatedAt < reference.CreatedAt
                    || (m.CreatedAt == reference.CreatedAt && m.MessageId < reference.MessageId));
            }
        }

        return await query
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.MessageId)
            .Take(pageSize)
            .ToListAsync();
    }

    public Task<ChatMessage?> GetMessageByIdAsync(long messageId)
    {
        return _context.ChatMessages
            .AsNoTracking()
            .SingleOrDefaultAsync(m => m.MessageId == messageId);
    }

    public Task<ChatMessage?> GetLatestMessageAsync(int roomId)
    {
        return _context.ChatMessages
            .AsNoTracking()
            .Where(m => m.RoomId == roomId)
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.MessageId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateMessageAsync(long messageId, string newContent, DateTime editedAt)
    {
        var message = await _context.ChatMessages.SingleOrDefaultAsync(m => m.MessageId == messageId);
        if (message == null)
        {
            throw new InvalidOperationException("Chat message not found.");
        }

        message.Content = newContent;
        message.EditedAt = editedAt;
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteMessageAsync(long messageId, DateTime deletedAt)
    {
        var message = await _context.ChatMessages.SingleOrDefaultAsync(m => m.MessageId == messageId);
        if (message == null)
        {
            throw new InvalidOperationException("Chat message not found.");
        }

        message.DeletedAt = deletedAt;
        await _context.SaveChangesAsync();
    }
}
