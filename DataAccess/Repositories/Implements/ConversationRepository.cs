using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly SchoolManagementDbContext _context;

        public ConversationRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        public async Task<Conversation> CreateConversationAsync(Conversation conversation)
        {
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task<Conversation?> GetConversationByIdAsync(int conversationId)
        {
            return await _context.Conversations
                .Include(c => c.ConversationParticipants)
                    .ThenInclude(cp => cp.User)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
        }

        public async Task<Conversation?> GetConversationByCourseIdAsync(int courseId)
        {
            return await _context.Conversations
                .Include(c => c.ConversationParticipants)
                    .ThenInclude(cp => cp.User)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(int userId)
        {
            return await _context.ConversationParticipants
                .Include(cp => cp.Conversation)
                    .ThenInclude(c => c.ConversationParticipants)
                        .ThenInclude(cp => cp.User)
                .Include(cp => cp.Conversation)
                    .ThenInclude(c => c.Course)
                .Where(cp => cp.UserId == userId && cp.LeftAt == null)
                .Select(cp => cp.Conversation)
                .Distinct()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task AddParticipantAsync(int conversationId, int userId)
        {
            var existingParticipant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (existingParticipant != null)
            {
                // Rejoin if previously left
                existingParticipant.LeftAt = null;
                existingParticipant.JoinedAt = DateTime.UtcNow;
            }
            else
            {
                var participant = new ConversationParticipant
                {
                    ConversationId = conversationId,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow
                };
                _context.ConversationParticipants.Add(participant);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveParticipantAsync(int conversationId, int userId)
        {
            var participant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (participant != null)
            {
                participant.LeftAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<int>> GetParticipantUserIdsAsync(int conversationId)
        {
            return await _context.ConversationParticipants
                .Where(cp => cp.ConversationId == conversationId && cp.LeftAt == null)
                .Select(cp => cp.UserId)
                .ToListAsync();
        }

        public async Task<bool> IsUserInConversationAsync(int conversationId, int userId)
        {
            return await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId 
                             && cp.UserId == userId 
                             && cp.LeftAt == null);
        }

        public async Task<int?> GetDirectConversationIdAsync(int userId1, int userId2)
        {
            return await _context.Conversations
                .Where(c => !c.IsGroup == true)
                .Where(c => c.ConversationParticipants.Count == 2)
                .Where(c => c.ConversationParticipants.Any(cp => cp.UserId == userId1 && cp.LeftAt == null))
                .Where(c => c.ConversationParticipants.Any(cp => cp.UserId == userId2 && cp.LeftAt == null))
                .Select(c => (int?)c.ConversationId)
                .FirstOrDefaultAsync();
        }
    }
}
