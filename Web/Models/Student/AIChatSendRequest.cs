namespace Web.Models.Student;

public class AIChatSendRequest
{
    public long SessionId { get; set; }
    public string Content { get; set; } = string.Empty;
}
