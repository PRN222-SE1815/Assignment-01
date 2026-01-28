using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models.Student;
using System.Security.Claims;

namespace Web.Controllers.Student;

[Authorize(Roles = nameof(UserRole.STUDENT))]
public class StudentAIChatController : Controller
{
    private readonly IAIChatService _aiChatService;

    public StudentAIChatController(IAIChatService aiChatService)
    {
        _aiChatService = aiChatService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var purposes = new List<SelectListItem>
        {
            new("Course Registration Support", "registration"),
            new("Grade Lookup", "grades"),
            new("Schedule Advisor", "schedule"),
            new("General Q&A", "general")
        };

        var viewModel = new AIChatIndexViewModel
        {
            Purposes = purposes
        };

        return View("~/Views/Student/AIChat/Index.cshtml", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartSession(string purpose)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        if (string.IsNullOrWhiteSpace(purpose))
        {
            TempData["Error"] = "Please select a conversation purpose.";
            return RedirectToAction(nameof(Index));
        }

        var session = await _aiChatService.StartSessionAsync(userId, purpose, null, null);
        if (session == null)
        {
            TempData["Error"] = "Unable to create chat session. Please try again.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Session), new { id = session.ChatSessionId });
    }

    [HttpGet]
    public async Task<IActionResult> Session(long id)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var session = await _aiChatService.GetSessionAsync(id, userId);
        if (session == null)
        {
            TempData["Error"] = "Chat session not found or you don't have access.";
            return RedirectToAction(nameof(Index));
        }

        var messages = await _aiChatService.GetRecentMessagesAsync(id, userId, 50);

        var viewModel = new AIChatSessionViewModel
        {
            ChatSessionId = session.ChatSessionId,
            Purpose = session.Purpose,
            Messages = messages
                .Where(m => m.SenderType != "SYSTEM")
                .Select(m => new AIChatMessageViewModel
                {
                    ChatMessageId = m.ChatMessageId,
                    SenderType = m.SenderType,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                })
                .ToList()
        };

        return View("~/Views/Student/AIChat/Session.cshtml", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send([FromBody] AIChatSendRequest request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized(new { error = "Please log in again." });
        }

        if (request == null || request.SessionId <= 0)
        {
            return BadRequest(new { error = "Invalid request." });
        }

        var result = await _aiChatService.SendUserMessageAsync(request.SessionId, userId, request.Content);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Message });
        }

        return Ok(new
        {
            success = true,
            message = new
            {
                chatMessageId = result.Data!.ChatMessageId,
                senderType = result.Data.SenderType,
                content = result.Data.Content,
                createdAt = result.Data.CreatedAt
            }
        });
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
