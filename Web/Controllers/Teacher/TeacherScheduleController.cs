using System.Security.Claims;
using BusinessLogic.DTOs.Request;
using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models.Teacher;

namespace Web.Controllers.Teacher;

[Authorize(Roles = nameof(UserRole.TEACHER))]
public class TeacherScheduleController : Controller
{
    private readonly IScheduleService _scheduleService;

    public TeacherScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CalendarViewMode? viewMode, DateOnly? anchorDate, int? semesterId)
    {
        var teacherId = GetTeacherId();
        var mode = viewMode ?? CalendarViewMode.WEEK;
        var anchor = anchorDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var (rangeStart, rangeEnd) = CalculateRange(mode, anchor);

        var request = new ScheduleQueryRequest
        {
            TeacherId = teacherId,
            SemesterId = semesterId ?? 0,
            ViewMode = mode,
            AnchorDate = anchor,
            RangeStart = rangeStart,
            RangeEnd = rangeEnd
        };

        var occurrences = await _scheduleService.GetTeacherScheduleAsync(request);
        var timeZone = ResolveTimeZone();

        var viewModel = new TeacherCalendarViewModel
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
                    g => g.Select(o => new TeacherScheduleOccurrenceViewModel
                    {
                        OccurrenceId = o.OccurrenceId,
                        Title = o.Title,
                        CourseName = o.CourseName,
                        SectionCode = o.SectionCode,
                        StartAtLocal = TimeZoneInfo.ConvertTimeFromUtc(o.StartAtUtc, timeZone),
                        EndAtLocal = TimeZoneInfo.ConvertTimeFromUtc(o.EndAtUtc, timeZone),
                        Location = o.Location,
                        OnlineUrl = o.OnlineUrl,
                        Status = o.Status,
                        IsOverride = o.IsOverride
                    }).ToList() as IReadOnlyList<TeacherScheduleOccurrenceViewModel>)
        };

        return View("~/Views/Teacher/Schedule/Index.cshtml", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> FeedJson(DateOnly? start, DateOnly? end, int? semesterId)
    {
        var teacherId = GetTeacherId();
        var rangeStart = start ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var rangeEnd = end ?? rangeStart.AddDays(42);

        var request = new ScheduleQueryRequest
        {
            TeacherId = teacherId,
            SemesterId = semesterId ?? 0,
            RangeStart = rangeStart,
            RangeEnd = rangeEnd
        };

        var occurrences = await _scheduleService.GetTeacherScheduleAsync(request);

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
                o.Location,
                o.Status
            }
        });

        return Json(events);
    }

    private int GetTeacherId()
    {
        var claim = User.FindFirst("TeacherId")?.Value;
        return int.TryParse(claim, out var teacherId) ? teacherId : 0;
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
