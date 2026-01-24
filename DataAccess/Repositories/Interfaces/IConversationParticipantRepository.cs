using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces
{
    public interface IConversationParticipantRepository
    {
        Task AddParticipantAsync(int conversationId, int userId);
        Task RemoveParticipantAsync(int conversationId, int userId);
        Task<List<int>> GetParticipantUserIdsAsync(int conversationId);
        
       
        Task<List<ConversationParticipant>> GetActiveParticipantsAsync(int conversationId);
        
        Task<bool> IsUserInConversationAsync(int conversationId, int userId);
        Task<int> GetParticipantCountAsync(int conversationId);
    }
}