using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;

namespace BusinessLogic.Services.Interfaces;

public interface IConversationService
{
    Task<ConversationResponse> CreateConversationAsync(CreateConversationRequest request);
    Task<List<ConversationResponse>> GetUserConversationsAsync(int userId);
    Task<ConversationResponse?> GetConversationDetailsAsync(int conversationId, int userId);
    Task<int> GetOrCreateDirectConversationAsync(int userId1, int userId2);
}
