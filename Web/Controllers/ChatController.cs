using BusinessLogic.DTOs.Request;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels;
using System.Security.Claims;

namespace Web.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var rooms = await _chatService.GetMyRoomsAsync(userId);
        var vm = new ChatRoomListVm { Rooms = rooms };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Room(int id)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var room = await _chatService.GetRoomAsync(id, userId);
        if (room == null)
        {
            TempData["Error"] = "You do not have access to this chat room.";
            return RedirectToAction(nameof(Index));
        }

        var messages = await _chatService.GetRoomMessagesAsync(id, userId, null, 50);

        var vm = new ChatRoomVm
        {
            Room = room,
            Messages = messages.Items,
            CurrentUserId = userId,
            CurrentUserRole = GetUserRole()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send([FromBody] SendMessageRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var attachments = request.Attachments?.Select(a => new ChatAttachmentInputDto
        {
            FileUrl = a.FileUrl,
            FileType = a.FileType,
            FileSizeBytes = a.FileSizeBytes
        }).ToList();

        var result = await _chatService.SendMessageAsync(request.RoomId, userId, request.Content, attachments);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message });
        }

        var messages = await _chatService.GetRoomMessagesAsync(request.RoomId, userId, null, 1);
        var latestMessage = messages.Items.LastOrDefault();

        return Ok(latestMessage);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromBody] EditMessageRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var result = await _chatService.EditMessageAsync(request.RoomId, request.MessageId, userId, request.NewContent);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message });
        }

        return Ok(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromBody] DeleteMessageRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var result = await _chatService.DeleteMessageAsync(request.RoomId, request.MessageId, userId);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message });
        }

        return Ok(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead([FromBody] MarkReadRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var result = await _chatService.MarkReadAsync(request.RoomId, userId, request.LastReadMessageId);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message });
        }

        return Ok(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Messages(int roomId, long? beforeMessageId, int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var messages = await _chatService.GetRoomMessagesAsync(roomId, userId, beforeMessageId, pageSize);
        return Ok(messages);
    }

    [HttpGet]
    public async Task<IActionResult> AvailableUsers(string? search)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var users = await _chatService.GetAvailableUsersForChatAsync(userId, search);
        return Ok(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var result = await _chatService.CreateGroupRoomAsync(userId, request.RoomName, request.MemberUserIds ?? new List<int>());
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDm([FromBody] CreateDmRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized();
        }

        var result = await _chatService.CreateOrGetDmRoomAsync(userId, request.OtherUserId);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message });
        }

        return Ok(result.Data);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }

    public sealed class SendMessageRequest
    {
        public int RoomId { get; set; }
        public string Content { get; set; } = string.Empty;
        public List<AttachmentInput>? Attachments { get; set; }
    }

    public sealed class AttachmentInput
    {
        public string FileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long? FileSizeBytes { get; set; }
    }

    public sealed class EditMessageRequest
    {
        public int RoomId { get; set; }
        public long MessageId { get; set; }
        public string NewContent { get; set; } = string.Empty;
    }

    public sealed class DeleteMessageRequest
    {
        public int RoomId { get; set; }
        public long MessageId { get; set; }
    }

    public sealed class MarkReadRequest
    {
        public int RoomId { get; set; }
        public long? LastReadMessageId { get; set; }
    }

    public sealed class CreateGroupRequest
    {
        public string RoomName { get; set; } = string.Empty;
        public List<int>? MemberUserIds { get; set; }
    }

    public sealed class CreateDmRequest
    {
        public int OtherUserId { get; set; }
    }
}
