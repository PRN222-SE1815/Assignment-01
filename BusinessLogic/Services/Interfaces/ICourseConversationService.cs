using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;

namespace BusinessLogic.Services.Interfaces
{
    public interface ICourseConversationService
    {
        Task<ConversationResponse> GetOrCreateCourseConversationAsync(int courseId);
        Task SyncCourseParticipantsAsync(int courseId);
    }
}