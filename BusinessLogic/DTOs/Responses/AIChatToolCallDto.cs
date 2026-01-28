namespace BusinessLogic.DTOs.Response;

public class AIChatToolCallDto
{
    public string ToolName { get; set; } = string.Empty;
    public string ArgumentsJson { get; set; } = string.Empty;
}
