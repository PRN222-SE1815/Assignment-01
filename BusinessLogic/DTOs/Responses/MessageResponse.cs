namespace BusinessLogic.DTOs.Responses;

public class MessageResponse
{
    public int MessageId { get; set; }
    public int ConversationId { get; set; }
    public int SenderUserId { get; set; }
    public string SenderName { get; set; }
    public string Body { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }
    public List<int> ReadByUserIds { get; set; } = new();
}