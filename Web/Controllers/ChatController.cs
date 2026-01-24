using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;
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
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;

        public ChatController(
            IConversationService conversationService,
            IMessageService messageService,
            ICourseConversationService courseConversationService,
            IStudyGroupService studyGroupService,
            INotificationService notificationService,
            IUserService userService,
            ICourseService courseService)
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _courseConversationService = courseConversationService;
            _studyGroupService = studyGroupService;
            _notificationService = notificationService;
            _userService = userService;
            _courseService = courseService;
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
        public async Task<IActionResult> CreateStudyGroup()
        {
            var userId = GetCurrentUserId();
            var userCourses = await _courseService.GetUserCoursesAsync(userId);
            ViewBag.UserCourses = userCourses;
            return View();
        }

        /// <summary>
        /// Create study group - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudyGroup(CreateStudyGroupRequest request)
        {
            // Debug: Log ALL form data received
            var formData = Request.Form;
            System.Diagnostics.Debug.WriteLine("=== CreateStudyGroup POST ===");
            foreach (var key in formData.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"Form[{key}] = {formData[key]}");
            }
            
            // Manually bind InvitedUserIds from form since model binding might not work
            if (formData.ContainsKey("InvitedUserIds"))
            {
                var invitedUserIdsStrings = formData["InvitedUserIds"];
                var invitedUserIds = new List<int>();
                foreach (var idStr in invitedUserIdsStrings)
                {
                    if (int.TryParse(idStr, out var id))
                    {
                        invitedUserIds.Add(id);
                    }
                }
                request.InvitedUserIds = invitedUserIds.ToArray();
                System.Diagnostics.Debug.WriteLine($"Manually bound InvitedUserIds: [{string.Join(", ", request.InvitedUserIds)}]");
            }
            
            System.Diagnostics.Debug.WriteLine($"Request object: GroupName={request.GroupName}, CourseId={request.CourseId}");
            System.Diagnostics.Debug.WriteLine($"InvitedUserIds is null: {request.InvitedUserIds == null}");
            if (request.InvitedUserIds != null)
            {
                System.Diagnostics.Debug.WriteLine($"InvitedUserIds count: {request.InvitedUserIds.Length}");
                System.Diagnostics.Debug.WriteLine($"InvitedUserIds: [{string.Join(", ", request.InvitedUserIds)}]");
            }
            
            if (!ModelState.IsValid)
            {
                // Reload courses for the view
                var userId = GetCurrentUserId();
                var userCourses = await _courseService.GetUserCoursesAsync(userId);
                ViewBag.UserCourses = userCourses;
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
                System.Diagnostics.Debug.WriteLine($"CreateStudyGroup Error: {ex.Message}");
                ModelState.AddModelError("", ex.Message);
                // Reload courses for the view
                var userId = GetCurrentUserId();
                var userCourses = await _courseService.GetUserCoursesAsync(userId);
                ViewBag.UserCourses = userCourses;
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
        /// Delete study group (Creator only)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteStudyGroup(int conversationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _studyGroupService.DeleteStudyGroupAsync(conversationId, userId);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Study group deleted successfully";
                    return RedirectToAction(nameof(StudyGroups));
                }
                
                TempData["ErrorMessage"] = "Failed to delete group. Only the creator can delete.";
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

        /// <summary>
        /// Search users for inviting to study group (only course participants)
        /// Use term='*' to get all members in the course
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string term, int? courseId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return Json(new List<object>());
                }

                if (!courseId.HasValue || courseId.Value <= 0)
                {
                    return Json(new { success = false, message = "Please select a course first" });
                }

                List<UserSearchResponse> users;
                
                // If term is '*', get all course participants
                if (term == "*")
                {
                    // Get all participants without search filter
                    var allParticipantIds = await _courseService.GetCourseParticipantUserIdsAsync(courseId.Value);
                    users = new List<UserSearchResponse>();
                    
                    foreach (var userId in allParticipantIds)
                    {
                        var user = await _userService.GetByIdAsync(userId);
                        if (user != null)
                        {
                            users.Add(new UserSearchResponse
                            {
                                UserId = user.UserId,
                                FullName = user.FullName,
                                Email = user.Email,
                                Username = user.Username,
                                RoleName = user.RoleName ?? "Unknown"
                            });
                        }
                    }
                }
                else
                {
                    // Search only within course participants
                    users = await _courseService.SearchCourseParticipantsAsync(courseId.Value, term);
                }
                
                // Format for Select2 or autocomplete
                var results = users.Select(u => new
                {
                    id = u.UserId,
                    text = $"{u.FullName} ({u.Email ?? u.Username})",
                    fullName = u.FullName,
                    email = u.Email,
                    username = u.Username,
                    role = u.RoleName
                });

                return Json(results);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Search all users for inviting to study group (no course filter)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchAllUsers(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 1)
                {
                    return Json(new List<object>());
                }

                var users = await _userService.SearchUsersAsync(term);
                
                // Exclude current user
                var currentUserId = GetCurrentUserId();
                var filteredUsers = users.Where(u => u.UserId != currentUserId);

                var results = filteredUsers.Select(u => new
                {
                    id = u.UserId,
                    fullName = u.FullName,
                    email = u.Email,
                    username = u.Username,
                    role = u.RoleName
                });

                return Json(results);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}

