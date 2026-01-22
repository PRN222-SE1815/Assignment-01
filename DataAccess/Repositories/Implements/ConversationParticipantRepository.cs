using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements
{
    public class ConversationParticipantRepository : IConversationParticipantRepository
    {
        private readonly SchoolManagementDbContext _context;

        public ConversationParticipantRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Thêm participant vào conversation
        /// Nếu user đã từng tham gia và left → rejoin
        /// </summary>
        public async Task AddParticipantAsync(int conversationId, int userId)
        {
            // Kiểm tra đã tồn tại chưa
            var existingParticipant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId 
                                        && cp.UserId == userId);

            if (existingParticipant != null)
            {
                // Nếu đã left trước đó → rejoin
                if (existingParticipant.LeftAt != null)
                {
                    existingParticipant.LeftAt = null;
                    existingParticipant.JoinedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                // Nếu đang active → không làm gì (idempotent)
            }
            else
            {
                // Tạo mới participant
                var participant = new ConversationParticipant
                {
                    ConversationId = conversationId,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow,
                    LeftAt = null
                };

                _context.ConversationParticipants.Add(participant);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Remove participant khỏi conversation (soft delete)
        /// Set LeftAt = current time
        /// </summary>
        public async Task RemoveParticipantAsync(int conversationId, int userId)
        {
            var participant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId 
                                        && cp.UserId == userId);

            if (participant != null && participant.LeftAt == null)
            {
                participant.LeftAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Lấy danh sách UserId của participants đang active
        /// </summary>
        public async Task<List<int>> GetParticipantUserIdsAsync(int conversationId)
        {
            return await _context.ConversationParticipants
                .Where(cp => cp.ConversationId == conversationId && cp.LeftAt == null)
                .Select(cp => cp.UserId)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách participants đang active với User information
        /// Include User để Service có thể dùng
        /// </summary>
        public async Task<List<ConversationParticipant>> GetActiveParticipantsAsync(int conversationId)
        {
            return await _context.ConversationParticipants
                .Where(cp => cp.ConversationId == conversationId && cp.LeftAt == null)
                .Include(cp => cp.User) // Include để Service không cần query thêm
                .OrderBy(cp => cp.JoinedAt) // Sắp xếp theo thứ tự join
                .ToListAsync();
        }

        /// <summary>
        /// Kiểm tra user có đang là participant active không
        /// </summary>
        public async Task<bool> IsUserInConversationAsync(int conversationId, int userId)
        {
            return await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId 
                             && cp.UserId == userId 
                             && cp.LeftAt == null);
        }

        /// <summary>
        /// Đếm số lượng participants đang active
        /// </summary>
        public async Task<int> GetParticipantCountAsync(int conversationId)
        {
            return await _context.ConversationParticipants
                .CountAsync(cp => cp.ConversationId == conversationId 
                                && cp.LeftAt == null);
        }

        /// <summary>
        /// Lấy tất cả participants (kể cả đã left) - for audit/history
        /// </summary>
        public async Task<List<ConversationParticipant>> GetAllParticipantsAsync(int conversationId)
        {
            return await _context.ConversationParticipants
                .Where(cp => cp.ConversationId == conversationId)
                .Include(cp => cp.User)
                .OrderBy(cp => cp.JoinedAt)
                .ToListAsync();
        }
    }
}
