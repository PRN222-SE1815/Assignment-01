using System;

namespace BusinessLogic.DTOs.Response;

public sealed class ChatMemberDto
{
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public string RoleInRoom { get; set; } = null!;
    public string MemberStatus { get; set; } = null!;
    public long? LastReadMessageId { get; set; }
    public DateTime JoinedAt { get; set; }
}
