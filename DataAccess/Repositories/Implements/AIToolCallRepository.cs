using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace DataAccess.Repositories.Implements;

public sealed class AIToolCallRepository : IAIToolCallRepository
{
    private readonly SchoolManagementDbContext _context;

    public AIToolCallRepository(SchoolManagementDbContext context)
    {
        _context = context;
    }

    public async Task<long> AddToolCallAsync(long sessionId, string toolName, string requestJson, string? responseJson, string status, DateTime createdAt)
    {
        var toolCall = new AIToolCall
        {
            ChatSessionId = sessionId,
            ToolName = toolName,
            RequestJson = requestJson,
            ResponseJson = responseJson,
            Status = status,
            CreatedAt = createdAt
        };

        _context.AIToolCalls.Add(toolCall);
        await _context.SaveChangesAsync();
        return toolCall.ToolCallId;
    }
}
