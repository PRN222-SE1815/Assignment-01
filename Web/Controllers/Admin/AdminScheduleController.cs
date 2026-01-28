using System.Security.Claims;
using BusinessLogic.DTOs.Request;
using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models.Admin;

namespace Web.Controllers.Admin;

[Authorize(Roles = nameof(UserRole.ADMIN))]
public class AdminScheduleController : Controller
{
    private readonly IScheduleService _scheduleService;

    public AdminScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int classSectionId)
    {
        var events = classSectionId > 0
            ? await _scheduleService.GetScheduleEventsAsync(classSectionId)
            : Array.Empty<BusinessLogic.DTOs.Response.AdminScheduleEventDto>();

        var viewModel = new ScheduleEventListViewModel
        {
            ClassSectionId = classSectionId,
            Events = events
        };

        return View("~/Views/Admin/Schedule/Index.cshtml", viewModel);
    }

    [HttpGet]
    public IActionResult Create(int classSectionId)
    {
        var viewModel = new ScheduleEventFormViewModel { ClassSectionId = classSectionId };
        return View("~/Views/Admin/Schedule/Create.cshtml", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ScheduleEventFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Schedule/Create.cshtml", model);
        }

        var request = new CreateScheduleEventRequest
        {
            ClassSectionId = model.ClassSectionId,
            Title = model.Title,
            StartAtUtc = model.StartAtUtc,
            EndAtUtc = model.EndAtUtc,
            Timezone = model.Timezone,
            Location = model.Location,
            OnlineUrl = model.OnlineUrl,
            TeacherId = model.TeacherId,
            RecurrenceRule = model.RecurrenceRule,
            RecurrenceStartDate = model.RecurrenceStartDate,
            RecurrenceEndDate = model.RecurrenceEndDate,
            CreatedBy = GetUserId()
        };

        var result = await _scheduleService.CreateScheduleEventAsync(request);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

        return RedirectToAction("Index", new { classSectionId = model.ClassSectionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(long scheduleEventId, int classSectionId)
    {
        var result = await _scheduleService.UpdateScheduleStatusAsync(new UpdateScheduleStatusRequest
        {
            ScheduleEventId = scheduleEventId,
            Status = ScheduleEventStatus.PUBLISHED.ToString(),
            UpdatedBy = GetUserId()
        });

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Index", new { classSectionId });
    }

    [HttpGet]
    public IActionResult Override(int recurrenceId, long scheduleEventId)
    {
        var viewModel = new ScheduleOverrideViewModel
        {
            RecurrenceId = recurrenceId,
            ScheduleEventId = scheduleEventId
        };
        return View("~/Views/Admin/Schedule/Override.cshtml", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Override(ScheduleOverrideViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Schedule/Override.cshtml", model);
        }

        var result = await _scheduleService.CreateOverrideAsync(new CreateScheduleOverrideRequest
        {
            ScheduleEventId = model.ScheduleEventId,
            RecurrenceId = model.RecurrenceId,
            OriginalDate = model.OriginalDate,
            OverrideType = model.OverrideType,
            NewStartAtUtc = model.NewStartAtUtc,
            NewEndAtUtc = model.NewEndAtUtc,
            NewLocation = model.NewLocation,
            NewTeacherId = model.NewTeacherId,
            Reason = model.Reason,
            ActorUserId = GetUserId()
        });

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Index", new { classSectionId = 0 });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long scheduleEventId, int classSectionId)
    {
        var eventDetail = await _scheduleService.GetScheduleEventDetailAsync(scheduleEventId);
        if (eventDetail == null)
        {
            TempData["ErrorMessage"] = "Schedule event not found.";
            return RedirectToAction("Index", new { classSectionId });
        }

        var viewModel = new ScheduleEventEditViewModel
        {
            ScheduleEventId = eventDetail.ScheduleEventId,
            ClassSectionId = eventDetail.ClassSectionId,
            Title = eventDetail.Title,
            StartAtUtc = eventDetail.StartAtUtc,
            EndAtUtc = eventDetail.EndAtUtc,
            Timezone = eventDetail.Timezone,
            Location = eventDetail.Location,
            OnlineUrl = eventDetail.OnlineUrl,
            TeacherId = eventDetail.TeacherId,
            Status = eventDetail.Status,
            RecurrenceId = eventDetail.RecurrenceId,
            RecurrenceRule = eventDetail.RecurrenceRule,
            RecurrenceStartDate = eventDetail.RecurrenceStartDate,
            RecurrenceEndDate = eventDetail.RecurrenceEndDate
        };

        return View("~/Views/Admin/Schedule/Edit.cshtml", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ScheduleEventEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Schedule/Edit.cshtml", model);
        }

        var request = new UpdateScheduleEventRequest
        {
            ScheduleEventId = model.ScheduleEventId,
            Title = model.Title,
            StartAtUtc = model.StartAtUtc,
            EndAtUtc = model.EndAtUtc,
            Timezone = model.Timezone,
            Location = model.Location,
            OnlineUrl = model.OnlineUrl,
            TeacherId = model.TeacherId,
            UpdatedBy = GetUserId(),
            Reason = model.Reason
        };

        var result = await _scheduleService.UpdateScheduleEventAsync(request);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

        return RedirectToAction("Index", new { classSectionId = model.ClassSectionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long scheduleEventId, int classSectionId, string? reason)
    {
        var result = await _scheduleService.UpdateScheduleStatusAsync(new UpdateScheduleStatusRequest
        {
            ScheduleEventId = scheduleEventId,
            Status = ScheduleEventStatus.CANCELLED.ToString(),
            UpdatedBy = GetUserId(),
            Reason = reason
        });

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Index", new { classSectionId });
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var userId) ? userId : 0;
    }
}
