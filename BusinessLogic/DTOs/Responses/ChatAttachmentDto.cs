using System;

namespace BusinessLogic.DTOs.Response;

public sealed class ChatAttachmentDto
{
    public long AttachmentId { get; set; }
    public long MessageId { get; set; }
    public string FileUrl { get; set; } = null!;
    public string FileType { get; set; } = null!;
    public long? FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}
