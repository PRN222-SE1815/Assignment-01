using BusinessLogic.DTOs.Response;

namespace BusinessLogic.Services.Interfaces;

public interface IAIChatService
{
    Task<AIChatSessionDto?> StartSessionAsync(int userId, string purpose, string? modelName, string? promptVersion);
    Task<AIChatSessionDto?> GetSessionAsync(long sessionId, int userId);
    Task<OperationResult<AIChatMessageDto>> SendUserMessageAsync(long sessionId, int userId, string content);
    Task<IReadOnlyList<AIChatMessageDto>> GetRecentMessagesAsync(long sessionId, int userId, int limit);
}
