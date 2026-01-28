using BusinessLogic.DTOs.Response;
using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models.Teacher;
using System.Security.Claims;

namespace Web.Controllers.Teacher;

[Authorize(Roles = nameof(UserRole.TEACHER))]
public class TeacherGradebookController : Controller
{
    private readonly IGradebookService _gradebookService;

    public TeacherGradebookController(IGradebookService gradebookService)
    {
        _gradebookService = gradebookService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var teacherId = GetTeacherId();
        if (teacherId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var sections = await _gradebookService.GetTeacherSectionsAsync(teacherId);
        var viewModel = new TeacherGradebookIndexViewModel
        {
            Sections = sections
        };

        return View("~/Views/Teacher/Gradebook/Index.cshtml", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var teacherId = GetTeacherId();
        if (teacherId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var matrix = await _gradebookService.GetGradebookAsync(id, teacherId);
        if (matrix == null)
        {
            TempData["Error"] = "Gradebook not found or you are not authorized to access it.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new TeacherGradebookEditViewModel
        {
            Gradebook = matrix.Gradebook,
            Items = matrix.Items,
            Students = matrix.Students,
            Entries = matrix.Entries,
            Stats = matrix.Stats
        };

        return View("~/Views/Teacher/Gradebook/Edit.cshtml", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveStructure(int id, [FromForm] List<GradeItemDto> items)
    {
        var teacherId = GetTeacherId();
        if (teacherId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _gradebookService.SaveStructureAsync(id, teacherId, items);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }
        else
        {
            TempData["Success"] = result.Message;
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveGrades(int id, [FromBody] SaveGradesRequest request)
    {
        var teacherId = GetTeacherId();
        if (teacherId == 0)
        {
            return Json(new { success = false, message = "Unauthorized" });
        }

        var result = await _gradebookService.SaveGradesAsync(id, teacherId, request.Entries, request.Reason);
        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var teacherId = GetTeacherId();
        if (teacherId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _gradebookService.PublishAsync(id, teacherId);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }
        else
        {
            TempData["Success"] = result.Message;
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(int id)
    {
        var teacherId = GetTeacherId();
        if (teacherId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _gradebookService.LockAsync(id, teacherId);
        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }
        else
        {
            TempData["Success"] = result.Message;
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    private int GetTeacherId()
    {
        var teacherIdClaim = User.FindFirst("TeacherId")?.Value;
        return int.TryParse(teacherIdClaim, out var teacherId) ? teacherId : 0;
    }
}

public class SaveGradesRequest
{
    public List<GradeEntryCellDto> Entries { get; set; } = new();
    public string? Reason { get; set; }
}
