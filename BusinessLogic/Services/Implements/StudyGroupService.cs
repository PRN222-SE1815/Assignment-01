using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements
{
    public class StudyGroupService : IStudyGroupService
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IConversationParticipantRepository _participantRepository;

        public StudyGroupService(
            IConversationRepository conversationRepository,
            IConversationParticipantRepository participantRepository)
        {
            _conversationRepository = conversationRepository;
            _participantRepository = participantRepository;
        }

        public async Task<ConversationResponse> CreateStudyGroupAsync(CreateStudyGroupRequest request)
        {
            var conversation = new Conversation
            {
                IsGroup = true,
                Title = request.GroupName,
                CreatedByUserId = request.CreatedByUserId,
                CreatedAt = DateTime.UtcNow,
                CourseId = null 
            };

            var savedConversation = await _conversationRepository.CreateConversationAsync(conversation);

            await _participantRepository.AddParticipantAsync(
                savedConversation.ConversationId,
                request.CreatedByUserId
            );

            if (request.InvitedUserIds != null && request.InvitedUserIds.Length > 0)
            {
                foreach (var userId in request.InvitedUserIds)
                {
                    await _participantRepository.AddParticipantAsync(
                        savedConversation.ConversationId,
                        userId
                    );
                }
            }

            // ✅ Dùng ParticipantRepository để lấy entities
            var participantEntities = await _participantRepository
                .GetActiveParticipantsAsync(savedConversation.ConversationId);

            // ✅ Mapping sang DTO trong Service
            var participants = participantEntities.Select(cp => new ParticipantResponse
            {
                UserId = cp.UserId,
                FullName = cp.User?.FullName ?? "Unknown",
                JoinedAt = cp.JoinedAt ?? DateTime.UtcNow
            }).ToList();

            return new ConversationResponse
            {
                ConversationId = savedConversation.ConversationId,
                IsGroup = savedConversation.IsGroup ?? false,
                Title = savedConversation.Title,
                CreatedByUserId = savedConversation.CreatedByUserId ?? 0,
                CreatedAt = savedConversation.CreatedAt ?? DateTime.UtcNow,
                CourseId = savedConversation.CourseId, // NULL for study groups
                Participants = participants
            };
        }

        public async Task<bool> InviteUserToGroupAsync(int conversationId, int invitedUserId, int inviterUserId)
        {
            // ✅ Dùng ParticipantRepository
            if (!await _participantRepository.IsUserInConversationAsync(conversationId, inviterUserId))
                return false;

            await _participantRepository.AddParticipantAsync(conversationId, invitedUserId);
            return true;
        }

        public async Task<bool> LeaveGroupAsync(int conversationId, int userId)
        {
            // ✅ Dùng ParticipantRepository
            await _participantRepository.RemoveParticipantAsync(conversationId, userId);
            return true;
        }

        public async Task<List<ConversationResponse>> GetUserStudyGroupsAsync(int userId)
        {
            var conversations = await _conversationRepository.GetUserConversationsAsync(userId);

            var result = new List<ConversationResponse>();

            foreach (var c in conversations.Where(c => c.CourseId == null && c.IsGroup == true))
            {
                // ✅ Lấy entities từ Repository
                var participantEntities = await _participantRepository
                    .GetActiveParticipantsAsync(c.ConversationId);

                // ✅ Mapping sang DTO trong Service
                var participants = participantEntities.Select(cp => new ParticipantResponse
                {
                    UserId = cp.UserId,
                    FullName = cp.User?.FullName ?? "Unknown",
                    JoinedAt = cp.JoinedAt ?? DateTime.UtcNow
                }).ToList();

                result.Add(new ConversationResponse
                {
                    ConversationId = c.ConversationId,
                    IsGroup = c.IsGroup ?? false,
                    Title = c.Title,
                    CreatedByUserId = c.CreatedByUserId ?? 0,
                    CreatedAt = c.CreatedAt ?? DateTime.UtcNow,
                    CourseId = c.CourseId, // NULL for study groups
                    Participants = participants
                });
            }

            return result;
        }

        public async Task<bool> DeleteStudyGroupAsync(int conversationId, int userId)
        {
            // Get conversation to check if user is creator
            var conversation = await _conversationRepository.GetConversationByIdAsync(conversationId);
            
            if (conversation == null)
                return false;

            // Only creator can delete
            if (conversation.CreatedByUserId != userId)
                return false;

            // Check if it's a study group (CourseId should be null)
            if (conversation.CourseId != null)
                return false; // Cannot delete course conversations

            // Remove all participants (this effectively deletes the group)
            var participants = await _participantRepository.GetActiveParticipantsAsync(conversationId);
            foreach (var participant in participants)
            {
                await _participantRepository.RemoveParticipantAsync(conversationId, participant.UserId);
            }

            return true;
        }
    }

}
