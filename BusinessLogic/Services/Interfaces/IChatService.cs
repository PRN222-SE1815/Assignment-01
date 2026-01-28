using BusinessLogic.DTOs.Request;
using BusinessLogic.DTOs.Response;

namespace BusinessLogic.Services.Interfaces;

public interface IChatService
{
    Task<IReadOnlyList<ChatRoomDto>> GetMyRoomsAsync(int userId);
    Task<ChatRoomDto?> GetRoomAsync(int roomId, int userId);
    Task<PagedResult<ChatMessageDto>> GetRoomMessagesAsync(int roomId, int userId, long? beforeMessageId, int pageSize);
    Task<ChatMessageDto?> GetLatestMessageAsync(int roomId, int userId);
    Task<OperationResult> SendMessageAsync(int roomId, int userId, string content, IReadOnlyList<ChatAttachmentInputDto>? attachments);
    Task<OperationResult> EditMessageAsync(int roomId, long messageId, int userId, string newContent);
    Task<OperationResult> DeleteMessageAsync(int roomId, long messageId, int userId);
    Task<OperationResult> MarkReadAsync(int roomId, int userId, long? lastReadMessageId);

    // Room creation methods
    Task<OperationResult<ChatRoomDto>> CreateGroupRoomAsync(int creatorUserId, string roomName, IReadOnlyList<int> memberUserIds);
    Task<OperationResult<ChatRoomDto>> CreateOrGetDmRoomAsync(int userId, int otherUserId);
    Task<IReadOnlyList<AvailableUserDto>> GetAvailableUsersForChatAsync(int userId, string? search);

    // Auto-join for enrollment
    Task EnsureClassChatMembershipAsync(int classSectionId, int studentId);
}
