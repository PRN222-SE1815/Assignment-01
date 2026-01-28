using BusinessLogic.DTOs.Response;

namespace BusinessLogic.Services.Interfaces;

public interface IGeminiClient
{
    Task<AIChatAssistantResponseDto> SendAsync(IReadOnlyList<AIChatMessageDto> messages, IReadOnlyList<AIChatToolDefinitionDto> tools, CancellationToken cancellationToken);
}
