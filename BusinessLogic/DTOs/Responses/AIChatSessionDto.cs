namespace BusinessLogic.DTOs.Response;

public class AIChatSessionDto
{
    public long ChatSessionId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? ModelName { get; set; }
    public string State { get; set; } = string.Empty;
    public string? PromptVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
