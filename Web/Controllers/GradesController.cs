using BusinessLogic.DTOs.Requests;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models;

namespace Web.Controllers;

[Authorize]
public class GradesController : Controller
{
    private readonly IGradeService _gradeService;

    public GradesController(IGradeService gradeService)
    {
        _gradeService = gradeService;
    }

    // Student + Teacher xem được
    [Authorize(Roles = "Student,Teacher")]
    public async Task<IActionResult> Index()
    {
        var grades = await _gradeService.GetAllAsync();
        return View(grades);
    }

    // Student + Teacher xem được
    [Authorize(Roles = "Student,Teacher")]
    public async Task<IActionResult> Details(int id)
    {
        var grade = await _gradeService.GetByIdAsync(id);
        if (grade == null) return NotFound();
        return View(grade);
    }

    // Teacher mới được tạo
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Create()
    {
        await LoadCourseDropdownAsync();
        ViewBag.Students = new List<SelectListItem>(); // sẽ load qua ajax
        return View(new GradeInputModel());
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GradeInputModel vm)
    {
        if (!ModelState.IsValid)
        {
            await LoadCourseDropdownAsync();
            await LoadStudentsDropdownByCourseAsync(vm.CourseId);
            return View(vm);
        }

        var req = new GradeUpsertRequest
        {
            EnrollmentId = vm.EnrollmentId,
            Assignment = vm.Assignment,
            Midterm = vm.Midterm,
            Final = vm.Final
        };

        await _gradeService.CreateAsync(req);
        return RedirectToAction(nameof(Index));
    }

    // Teacher mới được sửa
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Edit(int id)
    {
        var grade = await _gradeService.GetByIdAsync(id);
        if (grade == null) return NotFound();

        ViewBag.GradeId = id;

        var vm = new GradeInputModel
        {
            CourseId = grade.CourseId,          // ⚠️ GradeResponse phải có CourseId
            EnrollmentId = grade.EnrollmentId,
            Assignment = grade.Assignment,
            Midterm = grade.Midterm,
            Final = grade.Final
        };

        await LoadCourseDropdownAsync();
        await LoadStudentsDropdownByCourseAsync(vm.CourseId);

        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GradeInputModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.GradeId = id;
            await LoadCourseDropdownAsync();
            await LoadStudentsDropdownByCourseAsync(vm.CourseId);
            return View(vm);
        }

        var req = new GradeUpsertRequest
        {
            EnrollmentId = vm.EnrollmentId,
            Assignment = vm.Assignment,
            Midterm = vm.Midterm,
            Final = vm.Final
        };

        await _gradeService.UpdateAsync(id, req);
        return RedirectToAction(nameof(Index));
    }

    // GET: Grades/Delete/5
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        var grade = await _gradeService.GetByIdAsync(id);
        if (grade == null) return NotFound();
        return View(grade);
    }

    // POST: Grades/Delete/5
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _gradeService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }



    private async Task LoadCourseDropdownAsync()
    {
        var courses = await _gradeService.GetCourseOptionsAsync();
        ViewBag.Courses = courses.Select(c => new SelectListItem
        {
            Value = c.CourseId.ToString(),
            Text = c.CourseName
        }).ToList();
    }

    private async Task LoadStudentsDropdownByCourseAsync(int courseId)
    {
        if (courseId <= 0)
        {
            ViewBag.Students = new List<SelectListItem>();
            return;
        }

        var items = await _gradeService.GetEnrollmentOptionsByCourseAsync(courseId);
        ViewBag.Students = items.Select(x => new SelectListItem
        {
            Value = x.EnrollmentId.ToString(),
            Text = x.DisplayText
        }).ToList();
    }

    // AJAX: chọn course -> trả list student theo course
    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> StudentsByCourse(int courseId)
    {
        var items = await _gradeService.GetEnrollmentOptionsByCourseAsync(courseId);
        return Json(items.Select(x => new { enrollmentId = x.EnrollmentId, displayText = x.DisplayText }));
    }
}
