namespace BusinessLogic.DTOs.Responses
{
    public class ParticipantResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}