using BusinessLogic.DTOs.Request;
using BusinessLogic.DTOs.Response;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Web.Hubs;

[Authorize]
public sealed class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task JoinRoom(int roomId)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return;
        }

        var room = await _chatService.GetRoomAsync(roomId, userId);
        if (room == null)
        {
            await Clients.Caller.SendAsync("error", "You do not have access to this room.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"room-{roomId}");
        await Clients.Caller.SendAsync("joined", roomId);
    }

    public async Task LeaveRoom(int roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room-{roomId}");
    }

    public async Task SendMessage(int roomId, string content)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return;
        }

        var result = await _chatService.SendMessageAsync(roomId, userId, content, null);
        if (!result.Success)
        {
            await Clients.Caller.SendAsync("error", result.Message);
            return;
        }

        var message = await _chatService.GetLatestMessageAsync(roomId, userId);
        if (message != null)
        {
            await Clients.Group($"room-{roomId}").SendAsync("message:new", message);
        }
    }

    public async Task EditMessage(int roomId, long messageId, string newContent)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return;
        }

        var result = await _chatService.EditMessageAsync(roomId, messageId, userId, newContent);
        if (!result.Success)
        {
            await Clients.Caller.SendAsync("error", result.Message);
            return;
        }

        await Clients.Group($"room-{roomId}").SendAsync("message:edited", new { messageId, newContent });
    }

    public async Task DeleteMessage(int roomId, long messageId)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return;
        }

        var result = await _chatService.DeleteMessageAsync(roomId, messageId, userId);
        if (!result.Success)
        {
            await Clients.Caller.SendAsync("error", result.Message);
            return;
        }

        await Clients.Group($"room-{roomId}").SendAsync("message:deleted", new { messageId });
    }

    public async Task MarkRead(int roomId, long? lastReadMessageId)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return;
        }

        await _chatService.MarkReadAsync(roomId, userId, lastReadMessageId);
    }

    private int GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
