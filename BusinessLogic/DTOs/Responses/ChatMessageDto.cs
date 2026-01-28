using System;
using System.Collections.Generic;

namespace BusinessLogic.DTOs.Response;

public sealed class ChatMessageDto
{
    public long MessageId { get; set; }
    public int RoomId { get; set; }
    public int SenderId { get; set; }
    public string MessageType { get; set; } = null!;
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public IReadOnlyList<ChatAttachmentDto> Attachments { get; set; } = Array.Empty<ChatAttachmentDto>();
}
