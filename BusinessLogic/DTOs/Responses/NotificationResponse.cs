namespace BusinessLogic.DTOs.Responses;

public class NotificationResponse
{
    public int NotificationId { get; set; }
    public int? SenderUserId { get; set; }
    public string SenderName { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}