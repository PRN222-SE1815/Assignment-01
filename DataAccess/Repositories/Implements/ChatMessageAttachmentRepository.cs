using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public sealed class ChatMessageAttachmentRepository : IChatMessageAttachmentRepository
{
    private readonly SchoolManagementDbContext _context;

    public ChatMessageAttachmentRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task InsertAttachmentsAsync(IReadOnlyList<ChatMessageAttachment> attachments)
    {
        if (attachments.Count == 0)
        {
            return;
        }

        _context.ChatMessageAttachments.AddRange(attachments);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ChatMessageAttachment>> ListAttachmentsByMessageIdsAsync(IReadOnlyCollection<long> messageIds)
    {
        if (messageIds.Count == 0)
        {
            return Array.Empty<ChatMessageAttachment>();
        }

        return await _context.ChatMessageAttachments
            .AsNoTracking()
            .Where(a => messageIds.Contains(a.MessageId))
            .OrderBy(a => a.AttachmentId)
            .ToListAsync();
    }
}
