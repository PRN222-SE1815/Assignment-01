using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public sealed class ChatRoomMemberRepository : IChatRoomMemberRepository
{
    private readonly SchoolManagementDbContext _context;

    public ChatRoomMemberRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public Task<ChatRoomMember?> GetMembershipAsync(int roomId, int userId)
    {
        return _context.ChatRoomMembers
            .AsNoTracking()
            .SingleOrDefaultAsync(m => m.RoomId == roomId && m.UserId == userId);
    }

    public async Task<IReadOnlyList<ChatRoomMember>> ListMembersAsync(int roomId)
    {
        return await _context.ChatRoomMembers
            .AsNoTracking()
            .Where(m => m.RoomId == roomId)
            .OrderBy(m => m.UserId)
            .ToListAsync();
    }

    public async Task UpsertMembershipAsync(int roomId, int userId, string roleInRoom, string memberStatus, DateTime joinedAt)
    {
        var membership = await _context.ChatRoomMembers
            .SingleOrDefaultAsync(m => m.RoomId == roomId && m.UserId == userId);

        if (membership == null)
        {
            membership = new ChatRoomMember
            {
                RoomId = roomId,
                UserId = userId,
                RoleInRoom = roleInRoom,
                MemberStatus = memberStatus,
                JoinedAt = joinedAt
            };

            _context.ChatRoomMembers.Add(membership);
        }
        else
        {
            membership.RoleInRoom = roleInRoom;
            membership.MemberStatus = memberStatus;
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateMemberStatusAsync(int roomId, int userId, string memberStatus)
    {
        var membership = await _context.ChatRoomMembers
            .SingleOrDefaultAsync(m => m.RoomId == roomId && m.UserId == userId);
        if (membership == null)
        {
            throw new InvalidOperationException("Chat room member not found.");
        }

        membership.MemberStatus = memberStatus;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateLastReadMessageIdAsync(int roomId, int userId, long? lastReadMessageId)
    {
        var membership = await _context.ChatRoomMembers
            .SingleOrDefaultAsync(m => m.RoomId == roomId && m.UserId == userId);
        if (membership == null)
        {
            throw new InvalidOperationException("Chat room member not found.");
        }

        membership.LastReadMessageId = lastReadMessageId;
        await _context.SaveChangesAsync();
    }
}
