using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements
{
    public class MessageRepository : IMessageRepository
    {
        private readonly SchoolManagementDbContext _context;

        public MessageRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Message> AddMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(int conversationId, int skip = 0, int take = 50)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.IsDeleted != true)
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(take)
                .Include(m => m.MessageReads)
                .ToListAsync();
        }

        public async Task<Message?> GetMessageByIdAsync(int messageId)
        {
            return await _context.Messages
                .Include(m => m.MessageReads)
                .FirstOrDefaultAsync(m => m.MessageId == messageId);
        }

        public async Task UpdateMessageAsync(Message message)
        {
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> MarkAsReadAsync(int messageId, int userId)
        {
            var existingRead = await _context.MessageReads
                .FirstOrDefaultAsync(mr => mr.MessageId == messageId && mr.UserId == userId);

            if (existingRead != null)
                return false;

            var messageRead = new MessageRead
            {
                MessageId = messageId,
                UserId = userId,
                ReadAt = DateTime.UtcNow
            };

            _context.MessageReads.Add(messageRead);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<int>> GetReadByUserIdsAsync(int messageId)
        {
            return await _context.MessageReads
                .Where(mr => mr.MessageId == messageId)
                .Select(mr => mr.UserId)
                .ToListAsync();
        }
    }
}