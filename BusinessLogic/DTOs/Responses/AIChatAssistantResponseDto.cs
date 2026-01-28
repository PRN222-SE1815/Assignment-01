namespace BusinessLogic.DTOs.Response;

public class AIChatAssistantResponseDto
{
    public string Reply { get; set; } = string.Empty;
    public IReadOnlyList<AIChatToolCallDto> ToolCalls { get; set; } = Array.Empty<AIChatToolCallDto>();
}
