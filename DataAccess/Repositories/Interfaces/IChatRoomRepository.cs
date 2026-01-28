using DataAccess.Entities;

namespace DataAccess.Repositories.Interfaces;

public interface IChatRoomRepository
{
    Task<ChatRoom> CreateRoomAsync(string roomType, int? courseId, int? classSectionId, string roomName, int createdBy, DateTime createdAt);
    Task<ChatRoom?> GetRoomByIdAsync(int roomId);
    Task<ChatRoom?> GetRoomByTypeAndRefAsync(string roomType, int? courseId, int? classSectionId, int? userAId, int? userBId);
    Task<IReadOnlyList<ChatRoom>> ListRoomsForUserAsync(int userId);
    Task UpdateRoomStatusAsync(int roomId, string status);
}
