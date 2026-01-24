using System.Security.Claims;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ICourseScheduleService _scheduleService;

        public TeacherController(ICourseScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Teacher Dashboard";
            return View();
        }

        [HttpGet]
        public IActionResult ViewSchedule()
        {
            ViewData["Title"] = "My Teaching Schedule";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ScheduleEvents(string? start, string? end)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Json(new List<object>());
            }

            // Parse dates or use defaults
            var fromDate = !string.IsNullOrEmpty(start) && DateOnly.TryParse(start, out var parsedStart)
                ? parsedStart
                : DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));

            var toDate = !string.IsNullOrEmpty(end) && DateOnly.TryParse(end, out var parsedEnd)
                ? parsedEnd
                : DateOnly.FromDateTime(DateTime.Today.AddMonths(2));

            var events = await _scheduleService.GetTeacherCalendarAsync(userId, fromDate, toDate);
            return Json(events);
        }
    }
}

