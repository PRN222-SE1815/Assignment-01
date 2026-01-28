using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models.Student;

namespace Web.Controllers.Student;

[Authorize(Roles = nameof(UserRole.STUDENT))]
public class StudentGradesController : Controller
{
    private readonly IStudentGradeService _studentGradeService;

    public StudentGradesController(IStudentGradeService studentGradeService)
    {
        _studentGradeService = studentGradeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var studentId = GetStudentId();
        if (studentId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var sections = await _studentGradeService.GetMyGradeSectionsAsync(studentId);
        var viewModel = new StudentGradesIndexViewModel
        {
            Sections = sections
        };

        return View("~/Views/Student/Grades/Index.cshtml", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var studentId = GetStudentId();
        if (studentId == 0)
        {
            return RedirectToAction("Login", "Account");
        }

        var matrix = await _studentGradeService.GetMyGradeDetailsAsync(studentId, id);
        if (matrix == null)
        {
            TempData["Error"] = "Grades not found or not yet published.";
            return RedirectToAction(nameof(Index));
        }

        var student = matrix.Students.FirstOrDefault();
        if (student == null)
        {
            TempData["Error"] = "Student data not found.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new StudentGradeDetailsViewModel
        {
            Gradebook = matrix.Gradebook,
            Items = matrix.Items,
            Student = student,
            Entries = matrix.Entries
        };

        return View("~/Views/Student/Grades/Details.cshtml", viewModel);
    }

    private int GetStudentId()
    {
        var studentIdClaim = User.FindFirst("StudentId")?.Value;
        return int.TryParse(studentIdClaim, out var studentId) ? studentId : 0;
    }
}
