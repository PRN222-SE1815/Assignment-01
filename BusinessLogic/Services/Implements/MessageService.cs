
using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public MessageService(IMessageRepository messageRepository, IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public async Task<MessageResponse> SendMessageAsync(SendMessageRequest request)
        {
            var message = new Message
            {
                ConversationId = request.ConversationId,
                SenderUserId = request.SenderUserId,
                Body = request.Body,
                SentAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var savedMessage = await _messageRepository.AddMessageAsync(message);
            var sender = await _userRepository.GetByIdAsync(request.SenderUserId);

            return new MessageResponse
            {
                MessageId = savedMessage.MessageId,
                ConversationId = savedMessage.ConversationId,
                SenderUserId = savedMessage.SenderUserId,
                SenderName = sender?.FullName ?? "Unknown",
                Body = savedMessage.Body,
                SentAt = savedMessage.SentAt ?? DateTime.Now,
                EditedAt = savedMessage.EditedAt,
                IsDeleted = savedMessage.IsDeleted ?? false,
                ReadByUserIds = new List<int>()
            };
        }

        public async Task<List<MessageResponse>> GetConversationMessagesAsync(int conversationId, int userId, int skip = 0, int take = 50)
        {
            var messages = await _messageRepository.GetMessagesByConversationIdAsync(conversationId, skip, take);
            var responses = new List<MessageResponse>();

            foreach (var message in messages)
            {
                var sender = await _userRepository.GetByIdAsync(message.SenderUserId);
                var readByUserIds = await _messageRepository.GetReadByUserIdsAsync(message.MessageId);

                responses.Add(new MessageResponse
                {
                    MessageId = message.MessageId,
                    ConversationId = message.ConversationId,
                    SenderUserId = message.SenderUserId,
                    SenderName = sender?.FullName ?? "Unknown",
                    Body = message.Body,
                    SentAt = message.SentAt ?? DateTime.Now,
                    EditedAt = message.EditedAt,
                    IsDeleted = message.IsDeleted ?? false,
                    ReadByUserIds = readByUserIds
                });
            }

            return responses;
        }


public async Task<bool> MarkMessageAsReadAsync(int messageId, int userId)
{
    return await _messageRepository.MarkAsReadAsync(messageId, userId);
}

public async Task<MessageResponse?> EditMessageAsync(int messageId, int userId, string newBody)
{
    var message = await _messageRepository.GetMessageByIdAsync(messageId);
            
    if (message == null || message.SenderUserId != userId || message.IsDeleted == true)
        return null;

    message.Body = newBody;
    message.EditedAt = DateTime.UtcNow;
    await _messageRepository.UpdateMessageAsync(message);

    var sender = await _userRepository.GetByIdAsync(message.SenderUserId);
    var readByUserIds = await _messageRepository.GetReadByUserIdsAsync(message.MessageId);

    return new MessageResponse
    {
        MessageId = message.MessageId,
        ConversationId = message.ConversationId,
        SenderUserId = message.SenderUserId,
        SenderName = sender?.FullName ?? "Unknown",
        Body = message.Body,
        SentAt = message.SentAt ?? DateTime.Now,
        EditedAt = message.EditedAt,
        IsDeleted = message.IsDeleted ?? false,
        ReadByUserIds = readByUserIds
    };
}

public async Task<bool> DeleteMessageAsync(int messageId, int userId)
{
    var message = await _messageRepository.GetMessageByIdAsync(messageId);
            
    if (message == null || message.SenderUserId != userId)
        return false;

    message.IsDeleted = true;
    await _messageRepository.UpdateMessageAsync(message);
    return true;
}
}
}