using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IChatMessageRepository
{
    Task<long> InsertMessageAsync(int roomId, int senderId, string messageType, string? content, DateTime createdAt);
    Task<long> InsertMessageWithAttachmentsAsync(int roomId, int senderId, string messageType, string? content, DateTime createdAt, IReadOnlyList<ChatMessageAttachment> attachments);
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(int roomId, long? beforeMessageId, int pageSize);
    Task<ChatMessage?> GetMessageByIdAsync(long messageId);
    Task<ChatMessage?> GetLatestMessageAsync(int roomId);
    Task UpdateMessageAsync(long messageId, string newContent, DateTime editedAt);
    Task SoftDeleteMessageAsync(long messageId, DateTime deletedAt);
}
