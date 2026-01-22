namespace BusinessLogic.DTOs.Requests
{
    public class SendMessageRequest
    {
        public int ConversationId { get; set; }
        public int SenderUserId { get; set; }
        public string Body { get; set; }
    }
}