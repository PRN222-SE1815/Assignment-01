using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public sealed class ChatRoomRepository : IChatRoomRepository
{
    private readonly SchoolManagementDbContext _context;

    public ChatRoomRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task<ChatRoom> CreateRoomAsync(string roomType, int? courseId, int? classSectionId, string roomName, int createdBy, DateTime createdAt)
    {
        var room = new ChatRoom
        {
            RoomType = roomType,
            CourseId = courseId,
            ClassSectionId = classSectionId,
            RoomName = roomName,
            Status = "ACTIVE",
            CreatedBy = createdBy,
            CreatedAt = createdAt
        };

        _context.ChatRooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public Task<ChatRoom?> GetRoomByIdAsync(int roomId)
    {
        return _context.ChatRooms
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.RoomId == roomId);
    }

    public Task<ChatRoom?> GetRoomByTypeAndRefAsync(string roomType, int? courseId, int? classSectionId, int? userAId, int? userBId)
    {
        var query = _context.ChatRooms.AsNoTracking().Where(r => r.RoomType == roomType);

        return roomType switch
        {
            "CLASS" => query.SingleOrDefaultAsync(r => r.ClassSectionId == classSectionId),
            "COURSE" => query.SingleOrDefaultAsync(r => r.CourseId == courseId),
            "DM" => query.SingleOrDefaultAsync(r => userAId.HasValue && userBId.HasValue
                && r.ChatRoomMembers.Any(m => m.UserId == userAId.Value)
                && r.ChatRoomMembers.Any(m => m.UserId == userBId.Value)),
            _ => Task.FromResult<ChatRoom?>(null)
        };
    }

    public async Task<IReadOnlyList<ChatRoom>> ListRoomsForUserAsync(int userId)
    {
        return await _context.ChatRoomMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => m.Room)
            .OrderBy(r => r.RoomId)
            .ToListAsync();
    }

    public async Task UpdateRoomStatusAsync(int roomId, string status)
    {
        var room = await _context.ChatRooms.SingleOrDefaultAsync(r => r.RoomId == roomId);
        if (room == null)
        {
            throw new InvalidOperationException("Chat room not found.");
        }

        room.Status = status;
        await _context.SaveChangesAsync();
    }
}
