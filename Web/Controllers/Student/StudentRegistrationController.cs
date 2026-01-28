using System.Security.Claims;
using BusinessLogic.DTOs.Request;
using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models.Student;

namespace Web.Controllers.Student;

[Authorize(Roles = nameof(UserRole.STUDENT))]
public class StudentRegistrationController : Controller
{
    private readonly IEnrollmentService _enrollmentService;

    public StudentRegistrationController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? semesterId)
    {
        var sections = await _enrollmentService.GetOpenSectionsAsync(semesterId);
        var firstSection = sections.FirstOrDefault();

        var viewModel = new RegistrationIndexViewModel
        {
            SemesterId = firstSection?.SemesterId ?? semesterId ?? 0,
            SemesterName = firstSection?.SemesterName ?? string.Empty,
            Sections = sections
        };

        return View("~/Views/Student/Registration/Index.cshtml", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int classSectionId)
    {
        try
        {
            var studentId = GetStudentId();
            var result = await _enrollmentService.RegisterAsync(new RegisterCourseRequest
            {
                StudentId = studentId,
                ClassSectionId = classSectionId
            });

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        }
        catch
        {
            TempData["ErrorMessage"] = "Unable to register for the class section.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Drop(int enrollmentId)
    {
        try
        {
            var studentId = GetStudentId();
            var result = await _enrollmentService.DropAsync(new DropRequest
            {
                StudentId = studentId,
                EnrollmentId = enrollmentId
            });

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        }
        catch
        {
            TempData["ErrorMessage"] = "Unable to drop the course.";
        }

        return RedirectToAction("MyCourses");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(int enrollmentId)
    {
        try
        {
            var studentId = GetStudentId();
            var result = await _enrollmentService.WithdrawAsync(new WithdrawRequest
            {
                StudentId = studentId,
                EnrollmentId = enrollmentId
            });

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        }
        catch
        {
            TempData["ErrorMessage"] = "Unable to withdraw from the course.";
        }

        return RedirectToAction("MyCourses");
    }

    [HttpGet]
    public async Task<IActionResult> MyCourses(int? semesterId)
    {
        var studentId = GetStudentId();
        var courses = await _enrollmentService.GetMyCoursesAsync(studentId, semesterId);
        var first = courses.FirstOrDefault();

        var viewModel = new MyCoursesViewModel
        {
            SemesterId = first?.SemesterId ?? semesterId ?? 0,
            SemesterName = first?.SemesterName ?? string.Empty,
            Courses = courses
        };

        return View("~/Views/Student/Registration/MyCourses.cshtml", viewModel);
    }

    private int GetStudentId()
    {
        var claim = User.FindFirst("StudentId")?.Value;
        return int.TryParse(claim, out var studentId) ? studentId : 0;
    }
}
