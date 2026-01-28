namespace BusinessLogic.DTOs.Request;

public sealed class CreateChatRoomRequest
{
    public string RoomType { get; set; } = null!;
    public string RoomName { get; set; } = null!;
    public List<int>? MemberUserIds { get; set; }
}
