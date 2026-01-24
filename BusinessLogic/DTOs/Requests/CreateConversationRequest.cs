namespace BusinessLogic.DTOs.Requests;

public class CreateConversationRequest
{
    public bool IsGroup { get; set; }
    public string? Title { get; set; }
    public int CreatedByUserId { get; set; }
    public List<int> ParticipantUserIds { get; set; } = new();
}
