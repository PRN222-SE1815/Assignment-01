namespace BusinessLogic.DTOs.Responses
{
    public class ConversationResponse
    {
        public int ConversationId { get; set; }
        public bool IsGroup { get; set; }
        public string? Title { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CourseId { get; set; } // Null for study groups, NOT null for course conversations
        public MessageResponse? LastMessage { get; set; }
        public List<ParticipantResponse> Participants { get; set; } = new();
        public int UnreadCount { get; set; }
    }
    
}