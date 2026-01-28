namespace BusinessLogic.DTOs.Request;

public sealed class ChatAttachmentInputDto
{
    public string FileUrl { get; set; } = null!;
    public string FileType { get; set; } = null!;
    public long? FileSizeBytes { get; set; }
}
