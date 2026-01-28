namespace BusinessLogic.DTOs.Response;

public class AIChatMessageDto
{
    public long ChatMessageId { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
