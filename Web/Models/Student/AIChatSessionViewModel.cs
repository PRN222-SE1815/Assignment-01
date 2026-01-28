namespace Web.Models.Student;

public class AIChatSessionViewModel
{
    public long ChatSessionId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public IReadOnlyList<AIChatMessageViewModel> Messages { get; set; } = Array.Empty<AIChatMessageViewModel>();
}

public class AIChatMessageViewModel
{
    public long ChatMessageId { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
