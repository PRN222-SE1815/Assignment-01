using BusinessLogic.DTOs.Response;

namespace Web.ViewModels;

public sealed class ChatRoomListVm
{
    public IReadOnlyList<ChatRoomDto> Rooms { get; set; } = Array.Empty<ChatRoomDto>();
}
