using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IChatMessageAttachmentRepository
{
    Task InsertAttachmentsAsync(IReadOnlyList<ChatMessageAttachment> attachments);
    Task<IReadOnlyList<ChatMessageAttachment>> ListAttachmentsByMessageIdsAsync(IReadOnlyCollection<long> messageIds);
}
