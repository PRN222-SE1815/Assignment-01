using System.Security.Claims;
using BusinessLogic.DTOs.Request;
using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models.Student;

namespace Web.Controllers.Student;

[Authorize(Roles = nameof(UserRole.STUDENT))]
public class StudentScheduleController : Controller
{
    private readonly IScheduleService _scheduleService;

    public StudentScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CalendarViewMode? viewMode, DateOnly? anchorDate, int? semesterId)
    {
        var studentId = GetStudentId();
        var mode = viewMode ?? CalendarViewMode.WEEK;
        var anchor = anchorDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var (rangeStart, rangeEnd) = CalculateRange(mode, anchor);

        var request = new ScheduleQueryRequest
        {
            StudentId = studentId,
            SemesterId = semesterId ?? 0,
            ViewMode = mode,
            AnchorDate = anchor,
            RangeStart = rangeStart,
            RangeEnd = rangeEnd
        };

        var occurrences = await _scheduleService.GetStudentScheduleAsync(request);
        var timeZone = ResolveTimeZone();

        var viewModel = new StudentCalendarViewModel
        {
            ViewMode = mode,
            AnchorDate = anchor,
            RangeStart = rangeStart,
            RangeEnd = rangeEnd,
            OccurrencesByDay = occurrences
                .GroupBy(o => o.OccurrenceDate)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(o => new StudentScheduleOccurrenceViewModel
                    {
                        OccurrenceId = o.OccurrenceId,
                        Title = o.Title,
                        CourseName = o.CourseName,
                        SectionCode = o.SectionCode,
                        TeacherName = o.TeacherName,
                        StartAtLocal = TimeZoneInfo.ConvertTimeFromUtc(o.StartAtUtc, timeZone),
                        EndAtLocal = TimeZoneInfo.ConvertTimeFromUtc(o.EndAtUtc, timeZone),
                        Location = o.Location,
                        OnlineUrl = o.OnlineUrl,
                        Status = o.Status,
                        IsOverride = o.IsOverride
                    }).ToList() as IReadOnlyList<StudentScheduleOccurrenceViewModel>)
        };

        return View("~/Views/Student/Schedule/Index.cshtml", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> FeedJson(DateOnly? start, DateOnly? end, int? semesterId)
    {
        var studentId = GetStudentId();
        var rangeStart = start ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var rangeEnd = end ?? rangeStart.AddDays(42);

        var request = new ScheduleQueryRequest
        {
            StudentId = studentId,
            SemesterId = semesterId ?? 0,
            RangeStart = rangeStart,
            RangeEnd = rangeEnd
        };

        var occurrences = await _scheduleService.GetStudentScheduleAsync(request);

        var events = occurrences.Select(o => new
        {
            id = o.OccurrenceId,
            title = o.Title,
            start = o.StartAtUtc.ToString("o"),
            end = o.EndAtUtc.ToString("o"),
            extendedProps = new
            {
                o.CourseName,
                o.SectionCode,
                o.TeacherName,
                o.Location,
                o.Status
            }
        });

        return Json(events);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string occurrenceId)
    {
        // Parse occurrence id format: "{scheduleEventId}-{yyyyMMddHHmm}"
        var parts = occurrenceId?.Split('-');
        if (parts == null || parts.Length < 2 || !long.TryParse(parts[0], out var scheduleEventId))
        {
            return NotFound();
        }

        var eventDetail = await _scheduleService.GetScheduleEventDetailAsync(scheduleEventId);
        if (eventDetail == null)
        {
            return NotFound();
        }

        return PartialView("~/Views/Student/Schedule/_DetailsModal.cshtml", eventDetail);
    }

    private int GetStudentId()
    {
        var claim = User.FindFirst("StudentId")?.Value;
        return int.TryParse(claim, out var studentId) ? studentId : 0;
    }

    private static (DateOnly Start, DateOnly End) CalculateRange(CalendarViewMode mode, DateOnly anchor)
    {
        return mode switch
        {
            CalendarViewMode.TODAY => (anchor, anchor),
            CalendarViewMode.WEEK => (anchor.AddDays(-(int)anchor.DayOfWeek + 1), anchor.AddDays(7 - (int)anchor.DayOfWeek)),
            CalendarViewMode.MONTH => (new DateOnly(anchor.Year, anchor.Month, 1), new DateOnly(anchor.Year, anchor.Month, DateTime.DaysInMonth(anchor.Year, anchor.Month))),
            _ => (anchor, anchor.AddDays(6))
        };
    }

    private static TimeZoneInfo ResolveTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        }
        catch
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
    }
}
