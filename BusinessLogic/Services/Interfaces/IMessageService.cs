using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;

namespace BusinessLogic.Services.Interfaces;

public interface IMessageService
{
    Task<MessageResponse> SendMessageAsync(SendMessageRequest request);
    Task<List<MessageResponse>> GetConversationMessagesAsync(int conversationId, int userId, int skip = 0, int take = 50);
    Task<bool> MarkMessageAsReadAsync(int messageId, int userId);
    Task<MessageResponse?> EditMessageAsync(int messageId, int userId, string newBody);
    Task<bool> DeleteMessageAsync(int messageId, int userId);
}