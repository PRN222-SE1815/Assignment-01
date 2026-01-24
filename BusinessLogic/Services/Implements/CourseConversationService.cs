using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements
{
    public class CourseConversationService : ICourseConversationService
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly ICourseRepository _courseRepository; 
        public CourseConversationService(
            IConversationRepository conversationRepository,
            ICourseRepository courseRepository)
        {
            _conversationRepository = conversationRepository;
            _courseRepository = courseRepository;
        }

        public async Task<ConversationResponse> GetOrCreateCourseConversationAsync(int courseId)
        {
            //
            var course = await _courseRepository.GetCourseWithTeacherAsync(courseId);

            if (course == null)
                throw new Exception("Course not found");

            var conversation = await _conversationRepository.GetConversationByCourseIdAsync(courseId);

            if (conversation == null)
            {
                // Tạo mới conversation
                conversation = new Conversation
                {
                    IsGroup = true,
                    Title = $"{course.CourseCode} - {course.CourseName}",
                    CreatedByUserId = course.Teacher?.UserId,
                    CreatedAt = DateTime.UtcNow,
                    CourseId = courseId
                };

                conversation = await _conversationRepository.CreateConversationAsync(conversation);

                // Sync participants
                await SyncCourseParticipantsAsync(courseId);

                // Reload to get participants
                conversation = await _conversationRepository.GetConversationByIdAsync(conversation.ConversationId);
            }

            // Map to response
            var participants = conversation.ConversationParticipants
                .Where(cp => cp.LeftAt == null)
                .Select(cp => new ParticipantResponse
                {
                    UserId = cp.UserId,
                    FullName = cp.User?.FullName ?? "Unknown",
                    JoinedAt = cp.JoinedAt ?? DateTime.UtcNow
                })
                .ToList();

            return new ConversationResponse
            {
                ConversationId = conversation.ConversationId,
                IsGroup = conversation.IsGroup ?? false,
                Title = conversation.Title,
                CreatedByUserId = conversation.CreatedByUserId ?? 0,
                CreatedAt = conversation.CreatedAt ?? DateTime.UtcNow,
                Participants = participants
            };
        }

        public async Task SyncCourseParticipantsAsync(int courseId)
        {
            var conversation = await _conversationRepository.GetConversationByCourseIdAsync(courseId);
            if (conversation == null) return;

            // 
            var course = await _courseRepository.GetCourseWithTeacherAsync(courseId);
            
            // Add teacher
            if (course?.Teacher?.UserId > 0)
            {
                await _conversationRepository.AddParticipantAsync(
                    conversation.ConversationId, 
                    course.Teacher.UserId
                );
            }

            //  
            var studentUserIds = await _courseRepository.GetEnrolledStudentUserIdsAsync(courseId);

            foreach (var userId in studentUserIds)
            {
                await _conversationRepository.AddParticipantAsync(conversation.ConversationId, userId);
            }
        }
    }
}
