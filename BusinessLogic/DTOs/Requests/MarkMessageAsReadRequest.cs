namespace BusinessLogic.DTOs.Requests;

public class MarkMessageAsReadRequest
{
    public int MessageId { get; set; }
    public int UserId { get; set; }
}