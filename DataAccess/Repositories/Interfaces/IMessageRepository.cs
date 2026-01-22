using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IMessageRepository
{
    Task<Message> AddMessageAsync(Message message);
    Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId, int skip = 0, int take = 50);
    Task<Message?> GetMessageByIdAsync(int messageId);
    Task UpdateMessageAsync(Message message);
    Task<bool> MarkAsReadAsync(int messageId, int userId);
    Task<List<int>> GetReadByUserIdsAsync(int messageId);
}