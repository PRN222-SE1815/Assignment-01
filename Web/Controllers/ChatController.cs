using BusinessLogic.DTOs.Requests;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IConversationService _conversationService;
        private readonly IMessageService _messageService;
        private readonly ICourseConversationService _courseConversationService;
        private readonly IStudyGroupService _studyGroupService;
        private readonly INotificationService _notificationService;

        public ChatController(
            IConversationService conversationService,
            IMessageService messageService,
            ICourseConversationService courseConversationService,
            IStudyGroupService studyGroupService,
            INotificationService notificationService)
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _courseConversationService = courseConversationService;
            _studyGroupService = studyGroupService;
            _notificationService = notificationService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Main chat page - List all conversations
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var conversations = await _conversationService.GetUserConversationsAsync(userId);
            return View(conversations);
        }

        /// <summary>
        /// View conversation details with messages
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Conversation(int id)
        {
            var userId = GetCurrentUserId();
            var conversation = await _conversationService.GetConversationDetailsAsync(id, userId);

            if (conversation == null)
            {
                TempData["ErrorMessage"] = "Conversation not found or you don't have access.";
                return RedirectToAction(nameof(Index));
            }

            // Get messages
            var messages = await _messageService.GetConversationMessagesAsync(id, userId, 0, 100);
            
            ViewBag.Messages = messages;
            ViewBag.CurrentUserId = userId;
            return View(conversation);
        }

        /// <summary>
        /// Course chat - Get or create course conversation
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CourseChat(int courseId)
        {
            try
            {
                var conversation = await _courseConversationService.GetOrCreateCourseConversationAsync(courseId);
                return RedirectToAction(nameof(Conversation), new { id = conversation.ConversationId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Study groups list
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> StudyGroups()
        {
            var userId = GetCurrentUserId();
            var studyGroups = await _studyGroupService.GetUserStudyGroupsAsync(userId);
            return View(studyGroups);
        }

        /// <summary>
        /// Create study group - GET
        /// </summary>
        [HttpGet]
        public IActionResult CreateStudyGroup()
        {
            return View();
        }

        /// <summary>
        /// Create study group - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudyGroup(CreateStudyGroupRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                request.CreatedByUserId = GetCurrentUserId();
                var conversation = await _studyGroupService.CreateStudyGroupAsync(request);
                
                TempData["SuccessMessage"] = "Study group created successfully!";
                return RedirectToAction(nameof(Conversation), new { id = conversation.ConversationId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(request);
            }
        }

        /// <summary>
        /// Invite user to study group
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> InviteToGroup(int conversationId, int userId)
        {
            try
            {
                var inviterId = GetCurrentUserId();
                var success = await _studyGroupService.InviteUserToGroupAsync(conversationId, userId, inviterId);
                
                if (success)
                {
                    return Json(new { success = true, message = "User invited successfully" });
                }
                
                return Json(new { success = false, message = "Failed to invite user" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Leave study group
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LeaveGroup(int conversationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _studyGroupService.LeaveGroupAsync(conversationId, userId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "You left the study group";
                    return RedirectToAction(nameof(StudyGroups));
                }
                
                TempData["ErrorMessage"] = "Failed to leave group";
                return RedirectToAction(nameof(Conversation), new { id = conversationId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Conversation), new { id = conversationId });
            }
        }

        /// <summary>
        /// Get messages (AJAX endpoint for pagination)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMessages(int conversationId, int skip = 0, int take = 50)
        {
            try
            {
                var userId = GetCurrentUserId();
                var messages = await _messageService.GetConversationMessagesAsync(conversationId, userId, skip, take);
                return Json(new { success = true, messages });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Notifications page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return View(notifications);
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _notificationService.MarkAsReadAsync(notificationId, userId);
                return Json(new { success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get unread notification count (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _notificationService.GetUnreadCountAsync(userId);
                return Json(new { success = true, count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

