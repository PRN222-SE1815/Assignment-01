namespace BusinessLogic.DTOs.Responses
{
    public class ConversationResponse
    {
        public int ConversationId { get; set; }
        public bool IsGroup { get; set; }
        public string? Title { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ParticipantResponse> Participants { get; set; } = new();
    }
    
}