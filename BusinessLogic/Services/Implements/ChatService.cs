using BusinessLogic.DTOs.Request;
using BusinessLogic.DTOs.Response;
using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using System.Linq;

namespace BusinessLogic.Services.Implements;

public sealed class ChatService : IChatService
{
    private const int MaxMessageLength = 2000;

    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomMemberRepository _chatRoomMemberRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IChatMessageAttachmentRepository _chatMessageAttachmentRepository;
    private readonly IChatModerationLogRepository _chatModerationLogRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IClassSectionRepository _classSectionRepository;
    private readonly IUserRepository _userRepository;

    public ChatService(
        IChatRoomRepository chatRoomRepository,
        IChatRoomMemberRepository chatRoomMemberRepository,
        IChatMessageRepository chatMessageRepository,
        IChatMessageAttachmentRepository chatMessageAttachmentRepository,
        IChatModerationLogRepository chatModerationLogRepository,
        IEnrollmentRepository enrollmentRepository,
        IClassSectionRepository classSectionRepository,
        IUserRepository userRepository)
    {
        _chatRoomRepository = chatRoomRepository;
        _chatRoomMemberRepository = chatRoomMemberRepository;
        _chatMessageRepository = chatMessageRepository;
        _chatMessageAttachmentRepository = chatMessageAttachmentRepository;
        _chatModerationLogRepository = chatModerationLogRepository;
        _enrollmentRepository = enrollmentRepository;
        _classSectionRepository = classSectionRepository;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<ChatRoomDto>> GetMyRoomsAsync(int userId)
    {
        var rooms = await _chatRoomRepository.ListRoomsForUserAsync(userId);
        return rooms.Select(MapRoom).ToList();
    }

    public async Task<ChatRoomDto?> GetRoomAsync(int roomId, int userId)
    {
        var room = await _chatRoomRepository.GetRoomByIdAsync(roomId);
        if (room == null)
        {
            return null;
        }

        var membership = await EnsureMembershipAsync(room, userId);
        if (membership == null || IsReadBlocked(membership))
        {
            return null;
        }

        return MapRoom(room);
    }

    public async Task<PagedResult<ChatMessageDto>> GetRoomMessagesAsync(int roomId, int userId, long? beforeMessageId, int pageSize)
    {
        var room = await _chatRoomRepository.GetRoomByIdAsync(roomId);
        if (room == null)
        {
            return EmptyMessages(pageSize);
        }

        var membership = await EnsureMembershipAsync(room, userId);
        if (membership == null || IsReadBlocked(membership))
        {
            return EmptyMessages(pageSize);
        }

        var resolvedPageSize = pageSize <= 0 ? 20 : pageSize;
        var messages = await _chatMessageRepository.GetMessagesAsync(roomId, beforeMessageId, resolvedPageSize);
        var messageIds = messages.Select(m => m.MessageId).ToList();
        var attachments = await _chatMessageAttachmentRepository.ListAttachmentsByMessageIdsAsync(messageIds);
        var attachmentLookup = attachments.GroupBy(a => a.MessageId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ChatAttachmentDto>)g.Select(MapAttachment).ToList());

        var items = messages.Select(message =>
        {
            attachmentLookup.TryGetValue(message.MessageId, out var messageAttachments);
            return MapMessage(message, messageAttachments ?? Array.Empty<ChatAttachmentDto>());
        }).ToList();

        return new PagedResult<ChatMessageDto>
        {
            Page = 1,
            PageSize = resolvedPageSize,
            TotalCount = items.Count,
            Items = items
        };
    }

    public async Task<ChatMessageDto?> GetLatestMessageAsync(int roomId, int userId)
    {
        var room = await _chatRoomRepository.GetRoomByIdAsync(roomId);
        if (room == null)
        {
            return null;
        }

        var membership = await EnsureMembershipAsync(room, userId);
        if (membership == null || IsReadBlocked(membership))
        {
            return null;
        }

        var message = await _chatMessageRepository.GetLatestMessageAsync(roomId);
        if (message == null)
        {
            return null;
        }

        var attachments = await _chatMessageAttachmentRepository.ListAttachmentsByMessageIdsAsync(new[] { message.MessageId });
        var attachmentDtos = attachments.Select(MapAttachment).ToList();
        return MapMessage(message, attachmentDtos);
    }

    public async Task<OperationResult> SendMessageAsync(int roomId, int userId, string content, IReadOnlyList<ChatAttachmentInputDto>? attachments)
    {
        var room = await _chatRoomRepository.GetRoomByIdAsync(roomId);
        if (room == null)
        {
            return OperationResult.Failed("Chat room not found.");
        }

        var membership = await EnsureMembershipAsync(room, userId);
        if (membership == null || IsReadBlocked(membership))
        {
            return OperationResult.Failed("You do not have access to this room.");
        }

        if (IsSendBlocked(membership))
        {
            if (membership.MemberStatus == ChatRoomMemberStatus.MUTED.ToString())
            {
                await _chatModerationLogRepository.InsertModerationLogAsync(room.RoomId, userId, "MUTED_SEND_BLOCKED", userId, null, null, DateTime.UtcNow);
            }

            return OperationResult.Failed("You are not allowed to send messages in this room.");
        }

        if (room.Status == ChatRoomStatus.LOCKED.ToString()
            && membership.RoleInRoom != ChatRoomMemberRole.OWNER.ToString()
            && membership.RoleInRoom != ChatRoomMemberRole.MODERATOR.ToString())
        {
            return OperationResult.Failed("This room is locked.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return OperationResult.Failed("Message content is required.");
        }

        var trimmedContent = content.Trim();
        if (trimmedContent.Length > MaxMessageLength)
        {
            return OperationResult.Failed("Message content is too long.");
        }

        var createdAt = DateTime.UtcNow;
        var attachmentEntities = BuildAttachmentEntities(attachments, createdAt);

        if (attachmentEntities.Count == 0)
        {
            await _chatMessageRepository.InsertMessageAsync(roomId, userId, ChatMessageType.TEXT.ToString(), trimmedContent, createdAt);
        }
        else
        {
            await _chatMessageRepository.InsertMessageWithAttachmentsAsync(roomId, userId, ChatMessageType.TEXT.ToString(), trimmedContent, createdAt, attachmentEntities);
        }

        return OperationResult.Ok();
    }

    public async Task<OperationResult> EditMessageAsync(int roomId, long messageId, int userId, string newContent)
    {
        var room = await _chatRoomRepository.GetRoomByIdAsync(roomId);
        if (room == null)
        {
            return OperationResult.Failed("Chat room not found.");
        }

        var membership = await EnsureMembershipAsync(room, userId);
        if (membership == null || IsReadBlocked(membership))
        {
            return OperationResult.Failed("You do not have access to this room.");
        }

        if (string.IsNullOrWhiteSpace(newContent))
        {
            return OperationResult.Failed("Message content is required.");
        }

        var trimmedContent = newContent.Trim();
        if (trimmedContent.Length > MaxMessageLength)
        {
            return OperationResult.Failed("Message content is too long.");
        }

        var message = await _chatMessageRepository.GetMessageByIdAsync(messageId);
        if (message == null || message.RoomId != roomId)
        {
            return OperationResult.Failed("Message not found.");
        }

        if (message.SenderId != userId)
        {
            return OperationResult.Failed("You can only edit your own messages.");
        }

        if (message.DeletedAt.HasValue)
        {
            return OperationResult.Failed("This message has been deleted.");
        }

        await _chatMessageRepository.UpdateMessageAsync(messageId, trimmedContent, DateTime.UtcNow);
        return OperationResult.Ok();
    }

    public async Task<OperationResult> DeleteMessageAsync(int roomId, long messageId, int userId)
    {
        var room = await _chatRoomRepository.GetRoomByIdAsync(roomId);
        if (room == null)
        {
            return OperationResult.Failed("Chat room not found.");
        }

        var membership = await EnsureMembershipAsync(room, userId);
        if (membership == null || IsReadBlocked(membership))
        {
            return OperationResult.Failed("You do not have access to this room.");
        }

        var message = await _chatMessageRepository.GetMessageByIdAsync(messageId);
        if (message == null || message.RoomId != roomId)
        {
            return OperationResult.Failed("Message not found.");
        }

        if (message.SenderId != userId
            && membership.RoleInRoom != ChatRoomMemberRole.OWNER.ToString()
            && membership.RoleInRoom != ChatRoomMemberRole.MODERATOR.ToString())
        {
            return OperationResult.Failed("You are not allowed to delete this message.");
        }

        await _chatMessageRepository.SoftDeleteMessageAsync(messageId, DateTime.UtcNow);

        if (message.SenderId != userId)
        {
            await _chatModerationLogRepository.InsertModerationLogAsync(roomId, userId, "DELETE_MESSAGE", message.SenderId, messageId, null, DateTime.UtcNow);
        }

        return OperationResult.Ok();
    }

    public async Task<OperationResult> MarkReadAsync(int roomId, int userId, long? lastReadMessageId)
    {
        var room = await _chatRoomRepository.GetRoomByIdAsync(roomId);
        if (room == null)
        {
            return OperationResult.Failed("Chat room not found.");
        }

        var membership = await EnsureMembershipAsync(room, userId);
        if (membership == null || IsReadBlocked(membership))
        {
            return OperationResult.Failed("You do not have access to this room.");
        }

        await _chatRoomMemberRepository.UpdateLastReadMessageIdAsync(roomId, userId, lastReadMessageId);
        return OperationResult.Ok();
    }

    public async Task<OperationResult<ChatRoomDto>> CreateGroupRoomAsync(int creatorUserId, string roomName, IReadOnlyList<int> memberUserIds)
    {
        if (string.IsNullOrWhiteSpace(roomName))
        {
            return OperationResult<ChatRoomDto>.Failed("Room name is required.");
        }

        var trimmedName = roomName.Trim();
        if (trimmedName.Length > 100)
        {
            return OperationResult<ChatRoomDto>.Failed("Room name is too long.");
        }

        var createdAt = DateTime.UtcNow;
        var room = await _chatRoomRepository.CreateRoomAsync(
            ChatRoomType.GROUP.ToString(),
            null,
            null,
            trimmedName,
            creatorUserId,
            createdAt);

        await _chatRoomMemberRepository.UpsertMembershipAsync(
            room.RoomId,
            creatorUserId,
            ChatRoomMemberRole.OWNER.ToString(),
            ChatRoomMemberStatus.JOINED.ToString(),
            createdAt);

        foreach (var memberId in memberUserIds.Distinct().Where(id => id != creatorUserId))
        {
            await _chatRoomMemberRepository.UpsertMembershipAsync(
                room.RoomId,
                memberId,
                ChatRoomMemberRole.MEMBER.ToString(),
                ChatRoomMemberStatus.JOINED.ToString(),
                createdAt);
        }

        return OperationResult<ChatRoomDto>.Ok(MapRoom(room), "Group chat created successfully.");
    }

    public async Task<OperationResult<ChatRoomDto>> CreateOrGetDmRoomAsync(int userId, int otherUserId)
    {
        if (userId == otherUserId)
        {
            return OperationResult<ChatRoomDto>.Failed("Cannot create a DM with yourself.");
        }

        var otherUser = await _userRepository.GetUserByIdAsync(otherUserId);
        if (otherUser == null || !otherUser.IsActive)
        {
            return OperationResult<ChatRoomDto>.Failed("User not found.");
        }

        var existingRoom = await _chatRoomRepository.GetRoomByTypeAndRefAsync(
            ChatRoomType.DM.ToString(),
            null,
            null,
            userId,
            otherUserId);

        if (existingRoom != null)
        {
            return OperationResult<ChatRoomDto>.Ok(MapRoom(existingRoom));
        }

        var currentUser = await _userRepository.GetUserByIdAsync(userId);
        var roomName = $"{currentUser?.FullName ?? "User"} & {otherUser.FullName}";
        var createdAt = DateTime.UtcNow;

        var room = await _chatRoomRepository.CreateRoomAsync(
            ChatRoomType.DM.ToString(),
            null,
            null,
            roomName,
            userId,
            createdAt);

        await _chatRoomMemberRepository.UpsertMembershipAsync(
            room.RoomId,
            userId,
            ChatRoomMemberRole.MEMBER.ToString(),
            ChatRoomMemberStatus.JOINED.ToString(),
            createdAt);

        await _chatRoomMemberRepository.UpsertMembershipAsync(
            room.RoomId,
            otherUserId,
            ChatRoomMemberRole.MEMBER.ToString(),
            ChatRoomMemberStatus.JOINED.ToString(),
            createdAt);

        return OperationResult<ChatRoomDto>.Ok(MapRoom(room), "Direct message created.");
    }

    public async Task<IReadOnlyList<AvailableUserDto>> GetAvailableUsersForChatAsync(int userId, string? search)
    {
        var users = await _userRepository.SearchActiveUsersAsync(search, userId, 20);
        return users.Select(u => new AvailableUserDto
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Role = u.Role,
            Email = u.Email
        }).ToList();
    }

    public async Task EnsureClassChatMembershipAsync(int classSectionId, int studentId)
    {
        var room = await _chatRoomRepository.GetRoomByTypeAndRefAsync(
            ChatRoomType.CLASS.ToString(),
            null,
            classSectionId,
            null,
            null);

        if (room == null)
        {
            return;
        }

        var membership = await _chatRoomMemberRepository.GetMembershipAsync(room.RoomId, studentId);
        if (membership == null)
        {
            await _chatRoomMemberRepository.UpsertMembershipAsync(
                room.RoomId,
                studentId,
                ChatRoomMemberRole.MEMBER.ToString(),
                ChatRoomMemberStatus.JOINED.ToString(),
                DateTime.UtcNow);
        }
        else if (membership.MemberStatus == ChatRoomMemberStatus.READ_ONLY.ToString()
            || membership.MemberStatus == ChatRoomMemberStatus.REMOVED.ToString())
        {
            await _chatRoomMemberRepository.UpdateMemberStatusAsync(room.RoomId, studentId, ChatRoomMemberStatus.JOINED.ToString());
        }
    }

    private static ChatRoomDto MapRoom(ChatRoom room)
    {
        return new ChatRoomDto
        {
            RoomId = room.RoomId,
            RoomType = room.RoomType,
            CourseId = room.CourseId,
            ClassSectionId = room.ClassSectionId,
            RoomName = room.RoomName,
            Status = room.Status,
            CreatedBy = room.CreatedBy,
            CreatedAt = room.CreatedAt
        };
    }

    private static ChatAttachmentDto MapAttachment(ChatMessageAttachment attachment)
    {
        return new ChatAttachmentDto
        {
            AttachmentId = attachment.AttachmentId,
            MessageId = attachment.MessageId,
            FileUrl = attachment.FileUrl,
            FileType = attachment.FileType,
            FileSizeBytes = attachment.FileSizeBytes,
            CreatedAt = attachment.CreatedAt
        };
    }

    private static ChatMessageDto MapMessage(ChatMessage message, IReadOnlyList<ChatAttachmentDto> attachments)
    {
        return new ChatMessageDto
        {
            MessageId = message.MessageId,
            RoomId = message.RoomId,
            SenderId = message.SenderId,
            MessageType = message.MessageType,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            EditedAt = message.EditedAt,
            DeletedAt = message.DeletedAt,
            Attachments = attachments
        };
    }

    private static bool IsReadBlocked(ChatRoomMember member)
    {
        return member.MemberStatus == ChatRoomMemberStatus.BANNED.ToString()
            || member.MemberStatus == ChatRoomMemberStatus.REMOVED.ToString();
    }

    private static bool IsSendBlocked(ChatRoomMember member)
    {
        return member.MemberStatus == ChatRoomMemberStatus.BANNED.ToString()
            || member.MemberStatus == ChatRoomMemberStatus.REMOVED.ToString()
            || member.MemberStatus == ChatRoomMemberStatus.READ_ONLY.ToString()
            || member.MemberStatus == ChatRoomMemberStatus.MUTED.ToString();
    }

    private async Task<ChatRoomMember?> EnsureMembershipAsync(ChatRoom room, int userId)
    {
        var membership = await _chatRoomMemberRepository.GetMembershipAsync(room.RoomId, userId);
        if (membership != null)
        {
            return membership;
        }

        if (room.RoomType == ChatRoomType.CLASS.ToString())
        {
            if (!room.ClassSectionId.HasValue)
            {
                return null;
            }

            if (!await CanAccessClassRoomAsync(room.ClassSectionId.Value, userId))
            {
                return null;
            }
        }
        else if (room.RoomType == ChatRoomType.COURSE.ToString())
        {
            if (!room.CourseId.HasValue)
            {
                return null;
            }

            if (!await CanAccessCourseRoomAsync(room.CourseId.Value, userId))
            {
                return null;
            }
        }
        else
        {
            return null;
        }

        await _chatRoomMemberRepository.UpsertMembershipAsync(room.RoomId, userId, ChatRoomMemberRole.MEMBER.ToString(), ChatRoomMemberStatus.JOINED.ToString(), DateTime.UtcNow);
        return await _chatRoomMemberRepository.GetMembershipAsync(room.RoomId, userId);
    }

    private async Task<bool> CanAccessClassRoomAsync(int classSectionId, int userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (user.Role == UserRole.ADMIN.ToString())
        {
            return true;
        }

        if (user.Role == UserRole.TEACHER.ToString())
        {
            return await _classSectionRepository.IsTeacherAssignedAsync(classSectionId, userId);
        }

        if (user.Role == UserRole.STUDENT.ToString())
        {
            var enrollment = await _enrollmentRepository.GetEnrollmentBySectionAsync(userId, classSectionId);
            return enrollment?.Status == EnrollmentStatus.ENROLLED.ToString();
        }

        return false;
    }

    private async Task<bool> CanAccessCourseRoomAsync(int courseId, int userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (user.Role == UserRole.ADMIN.ToString())
        {
            return true;
        }

        if (user.Role == UserRole.TEACHER.ToString())
        {
            return await _classSectionRepository.IsTeacherAssignedToCourseAsync(userId, courseId);
        }

        if (user.Role == UserRole.STUDENT.ToString())
        {
            return await _enrollmentRepository.IsStudentEnrolledInCourseAsync(userId, courseId, new[]
            {
                EnrollmentStatus.ENROLLED.ToString()
            });
        }

        return false;
    }

    private static List<ChatMessageAttachment> BuildAttachmentEntities(IReadOnlyList<ChatAttachmentInputDto>? attachments, DateTime createdAt)
    {
        if (attachments == null || attachments.Count == 0)
        {
            return new List<ChatMessageAttachment>();
        }

        var results = new List<ChatMessageAttachment>();
        foreach (var attachment in attachments)
        {
            if (string.IsNullOrWhiteSpace(attachment.FileUrl) || string.IsNullOrWhiteSpace(attachment.FileType))
            {
                continue;
            }

            results.Add(new ChatMessageAttachment
            {
                FileUrl = attachment.FileUrl.Trim(),
                FileType = attachment.FileType.Trim(),
                FileSizeBytes = attachment.FileSizeBytes,
                CreatedAt = createdAt
            });
        }

        return results;
    }

    private static PagedResult<ChatMessageDto> EmptyMessages(int pageSize)
    {
        return new PagedResult<ChatMessageDto>
        {
            Page = 1,
            PageSize = pageSize <= 0 ? 20 : pageSize,
            TotalCount = 0,
            Items = Array.Empty<ChatMessageDto>()
        };
    }
}
