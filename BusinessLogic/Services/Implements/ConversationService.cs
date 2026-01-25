using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public ConversationService(
            IConversationRepository conversationRepository,
            IMessageRepository messageRepository,
            IUserRepository userRepository)
        {
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public async Task<ConversationResponse> CreateConversationAsync(CreateConversationRequest request)
        {
            var conversation = new Conversation
            {
                IsGroup = request.IsGroup,
                Title = request.Title,
                CreatedByUserId = request.CreatedByUserId,
                CreatedAt = DateTime.UtcNow
            };

            var savedConversation = await _conversationRepository.CreateConversationAsync(conversation);


            foreach (var userId in request.ParticipantUserIds)
            {
                await _conversationRepository.AddParticipantAsync(savedConversation.ConversationId, userId);
            }

            return await GetConversationDetailsAsync(savedConversation.ConversationId, request.CreatedByUserId)
                   ?? new ConversationResponse();
        }

        public async Task<List<ConversationResponse>> GetUserConversationsAsync(int userId)
        {
            var conversations = await _conversationRepository.GetUserConversationsAsync(userId);
            var responses = new List<ConversationResponse>();

            foreach (var conversation in conversations)
            {
                var detail = await GetConversationDetailsAsync(conversation.ConversationId, userId);
                if (detail != null)
                    responses.Add(detail);
            }

            return responses;
        }

        public async Task<ConversationResponse?> GetConversationDetailsAsync(int conversationId, int userId)
        {
            var conversation = await _conversationRepository.GetConversationByIdAsync(conversationId);
            if (conversation == null)
                return null;

            var participants = new List<ParticipantResponse>();
            foreach (var cp in conversation.ConversationParticipants.Where(cp => cp.LeftAt == null))
            {
                participants.Add(new ParticipantResponse
                {
                    UserId = cp.UserId,
                    FullName = cp.User?.FullName ?? "Unknown",
                    JoinedAt = cp.JoinedAt ?? DateTime.UtcNow
                });
            }

            var lastMessages = await _messageRepository.GetMessagesByConversationIdAsync(conversationId, 0, 1);
            MessageResponse? lastMessage = null;

            if (lastMessages.Any())
            {
                var msg = lastMessages.First();
                var sender = await _userRepository.GetByIdAsync(msg.SenderUserId);
                lastMessage = new MessageResponse
                {
                    MessageId = msg.MessageId,
                    ConversationId = msg.ConversationId,
                    SenderUserId = msg.SenderUserId,
                    SenderName = sender?.FullName ?? "Unknown",
                    Body = msg.Body,
                    SentAt = msg.SentAt ?? DateTime.Now,
                };
            }

            return new ConversationResponse
            {
                ConversationId = conversation.ConversationId,
                IsGroup = conversation.IsGroup ?? false,
                Title = conversation.Title,
                CreatedByUserId = conversation.CreatedByUserId ?? 0,
                CreatedAt = conversation.CreatedAt ?? DateTime.UtcNow,
                CourseId = conversation.CourseId,
                Participants = participants,
                LastMessage = lastMessage,
                UnreadCount = 0
            };
        }

        public async Task<int> GetOrCreateDirectConversationAsync(int userId1, int userId2)
        {
            var existingId = await _conversationRepository.GetDirectConversationIdAsync(userId1, userId2);
            if (existingId.HasValue)
                return existingId.Value;

            var request = new CreateConversationRequest
            {
                IsGroup = false,
                CreatedByUserId = userId1,
                ParticipantUserIds = new List<int> { userId1, userId2 }
            };

            var conversation = await CreateConversationAsync(request);
            return conversation.ConversationId;
        }
    }
}
