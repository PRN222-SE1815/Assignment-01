using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;

namespace BusinessLogic.Services.Interfaces
{
    public interface IStudyGroupService
    {
        Task<ConversationResponse> CreateStudyGroupAsync(CreateStudyGroupRequest request);
        Task<bool> InviteUserToGroupAsync(int conversationId, int invitedUserId, int inviterUserId);
        Task<bool> LeaveGroupAsync(int conversationId, int userId);
        Task<List<ConversationResponse>> GetUserStudyGroupsAsync(int userId);
    }
}