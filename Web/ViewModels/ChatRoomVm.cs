using BusinessLogic.DTOs.Response;

namespace Web.ViewModels;

public sealed class ChatRoomVm
{
    public ChatRoomDto Room { get; set; } = null!;
    public IReadOnlyList<ChatMessageDto> Messages { get; set; } = Array.Empty<ChatMessageDto>();
    public int CurrentUserId { get; set; }
    public string CurrentUserRole { get; set; } = string.Empty;
}
