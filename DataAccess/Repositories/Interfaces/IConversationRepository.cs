using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IConversationRepository
{
    Task<Conversation> CreateConversationAsync(Conversation conversation);
    Task<Conversation?> GetConversationByIdAsync(int conversationId);
    Task<Conversation?> GetConversationByCourseIdAsync(int courseId);
    Task<List<Conversation>> GetUserConversationsAsync(int userId);
    Task AddParticipantAsync(int conversationId, int userId);
    Task RemoveParticipantAsync(int conversationId, int userId);
    Task<List<int>> GetParticipantUserIdsAsync(int conversationId);
    Task<bool> IsUserInConversationAsync(int conversationId, int userId);
}