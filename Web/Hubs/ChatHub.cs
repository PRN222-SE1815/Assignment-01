using BusinessLogic.DTOs.Requests;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IConversationService _conversationService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(
            IMessageService messageService,
            IConversationService conversationService,
            INotificationService notificationService,
            ILogger<ChatHub> logger)
        {
            _messageService = messageService;
            _conversationService = conversationService;
            _notificationService = notificationService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            if (userId > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                _logger.LogInformation($"User {userId} connected with ConnectionId {Context.ConnectionId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            if (userId > 0)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
                _logger.LogInformation($"User {userId} disconnected");
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a conversation group
        /// </summary>
        public async Task JoinConversation(int conversationId)
        {
            var userId = GetCurrentUserId();
            var conversation = await _conversationService.GetConversationDetailsAsync(conversationId, userId);
            
            if (conversation == null)
            {
                await Clients.Caller.SendAsync("Error", "Conversation not found");
                return;
            }

            // Check if user is participant
            if (!conversation.Participants.Any(p => p.UserId == userId))
            {
                await Clients.Caller.SendAsync("Error", "You are not a participant of this conversation");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
            _logger.LogInformation($"User {userId} joined conversation {conversationId}");
        }

        /// <summary>
        /// Leave a conversation group
        /// </summary>
        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
            var userId = GetCurrentUserId();
            _logger.LogInformation($"User {userId} left conversation {conversationId}");
        }

        /// <summary>
        /// Send a message to a conversation
        /// </summary>
        public async Task SendMessage(int conversationId, string messageBody)
        {
            try
            {
                var userId = GetCurrentUserId();

                var request = new SendMessageRequest
                {
                    ConversationId = conversationId,
                    SenderUserId = userId,
                    Body = messageBody
                };

                var messageResponse = await _messageService.SendMessageAsync(request);

                // Broadcast to all participants in conversation
                await Clients.Group($"Conversation_{conversationId}")
                    .SendAsync("ReceiveMessage", messageResponse);

                _logger.LogInformation($"User {userId} sent message {messageResponse.MessageId} to conversation {conversationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        /// <summary>
        /// Mark message as read
        /// </summary>
        public async Task MarkMessageAsRead(int messageId, int conversationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _messageService.MarkMessageAsReadAsync(messageId, userId);

                if (success)
                {
                    // Notify sender that message was read
                    await Clients.Group($"Conversation_{conversationId}")
                        .SendAsync("MessageRead", new { messageId, userId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read");
            }
        }

        /// <summary>
        /// Edit a message
        /// </summary>
        public async Task EditMessage(int messageId, int conversationId, string newBody)
        {
            try
            {
                var userId = GetCurrentUserId();
                var updatedMessage = await _messageService.EditMessageAsync(messageId, userId, newBody);

                if (updatedMessage != null)
                {
                    await Clients.Group($"Conversation_{conversationId}")
                        .SendAsync("MessageEdited", updatedMessage);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Failed to edit message");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing message");
                await Clients.Caller.SendAsync("Error", "Failed to edit message");
            }
        }

        /// <summary>
        /// Delete a message
        /// </summary>
        public async Task DeleteMessage(int messageId, int conversationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _messageService.DeleteMessageAsync(messageId, userId);

                if (success)
                {
                    await Clients.Group($"Conversation_{conversationId}")
                        .SendAsync("MessageDeleted", messageId);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Failed to delete message");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message");
                await Clients.Caller.SendAsync("Error", "Failed to delete message");
            }
        }

        /// <summary>
        /// Typing indicator
        /// </summary>
        public async Task UserTyping(int conversationId, string userName)
        {
            await Clients.OthersInGroup($"Conversation_{conversationId}")
                .SendAsync("UserTyping", new { userName, conversationId });
        }

        /// <summary>
        /// Stop typing indicator
        /// </summary>
        public async Task UserStoppedTyping(int conversationId, string userName)
        {
            await Clients.OthersInGroup($"Conversation_{conversationId}")
                .SendAsync("UserStoppedTyping", new { userName, conversationId });
        }

        /// <summary>
        /// Send notification to specific users
        /// </summary>
        public async Task SendNotification(List<int> recipientUserIds, string title, string message)
        {
            try
            {
                var senderId = GetCurrentUserId();
                await _notificationService.SendNotificationAsync(senderId, recipientUserIds, title, message);

                // Notify recipients via SignalR
                foreach (var recipientId in recipientUserIds)
                {
                    await Clients.Group($"User_{recipientId}")
                        .SendAsync("ReceiveNotification", new { title, message, senderId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
            }
        }
    }
}

