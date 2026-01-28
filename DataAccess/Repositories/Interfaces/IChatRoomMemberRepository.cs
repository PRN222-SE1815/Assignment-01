using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IChatRoomMemberRepository
{
    Task<ChatRoomMember?> GetMembershipAsync(int roomId, int userId);
    Task<IReadOnlyList<ChatRoomMember>> ListMembersAsync(int roomId);
    Task UpsertMembershipAsync(int roomId, int userId, string roleInRoom, string memberStatus, DateTime joinedAt);
    Task UpdateMemberStatusAsync(int roomId, int userId, string memberStatus);
    Task UpdateLastReadMessageIdAsync(int roomId, int userId, long? lastReadMessageId);
}
