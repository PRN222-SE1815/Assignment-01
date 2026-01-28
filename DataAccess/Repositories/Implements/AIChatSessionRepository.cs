using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories.Implements;

public sealed class AIChatSessionRepository : IAIChatSessionRepository
{
    private readonly SchoolManagementDbContext _context;

    public AIChatSessionRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task<long> CreateSessionAsync(int userId, string purpose, string? modelName, string? promptVersion, DateTime createdAt)
    {
        var session = new AIChatSession
        {
            UserId = userId,
            Purpose = purpose,
            ModelName = modelName,
            State = "ACTIVE",
            PromptVersion = promptVersion,
            CreatedAt = createdAt
        };

        _context.AIChatSessions.Add(session);
        await _context.SaveChangesAsync();
        return session.ChatSessionId;
    }

    public Task<AIChatSession?> GetByIdAsync(long sessionId)
    {
        return _context.AIChatSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.ChatSessionId == sessionId);
    }

    public async Task CompleteSessionAsync(long sessionId, string state, DateTime completedAt)
    {
        var session = await _context.AIChatSessions.SingleOrDefaultAsync(s => s.ChatSessionId == sessionId);
        if (session == null)
        {
            throw new InvalidOperationException("AI chat session not found.");
        }

        session.State = state;
        session.CompletedAt = completedAt;
        await _context.SaveChangesAsync();
    }
}
