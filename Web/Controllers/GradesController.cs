using BusinessLogic.DTOs.Requests;
using BusinessLogic.Services.Implements;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
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
    [Authorize(Roles = "Student,Teacher,Admin")]
    public async Task<IActionResult> Index(int? courseId, string? student)
    {
        // Đổ dropdown course
        await LoadCourseDropdownAsync();

        // Lấy toàn bộ grade
        var grades = await _gradeService.GetAllAsync();

        // Filter theo course
        if (courseId.HasValue && courseId.Value > 0)
        {
            grades = grades.Where(g => g.CourseId == courseId.Value).ToList();
        }

        // Filter theo tên sinh viên
        if (!string.IsNullOrWhiteSpace(student))
        {
            var key = student.Trim().ToLower();
            grades = grades.Where(g => (g.StudentName ?? "").ToLower().Contains(key)).ToList();
        }

        // Giữ lại giá trị search để view hiển thị lại
        ViewBag.SelectedCourseId = courseId;
        ViewBag.StudentKeyword = student;

        return View(grades);
    }


    // Student + Teacher xem được
    [Authorize(Roles = "Student,Teacher,Admin")]
    public async Task<IActionResult> Details(int id)
    {
        var grade = await _gradeService.GetByIdAsync(id);
        if (grade == null) return NotFound();
        return View(grade);
    }

    // Teacher mới được tạo
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> Create()
    {
        await LoadCourseDropdownAsync();
        ViewBag.Students = new List<SelectListItem>(); // sẽ load qua ajax
        return View(new GradeInputModel());
    }

    [HttpPost]
    [Authorize(Roles = "Teacher,Admin")]
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
    [Authorize(Roles = "Teacher,Admin")]
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
    [Authorize(Roles = "Teacher,Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var grade = await _gradeService.GetByIdAsync(id);
        if (grade == null) return NotFound();
        return View(grade);
    }

    // POST: Grades/Delete/5
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Teacher,Admin")]
    public async Task<IActionResult> StudentsByCourse(int courseId)
    {
        var items = await _gradeService.GetEnrollmentOptionsByCourseAsync(courseId);
        return Json(items.Select(x => new { enrollmentId = x.EnrollmentId, displayText = x.DisplayText }));
    }
    

[Authorize(Roles = "Student")]
public async Task<IActionResult> MyGrades(int? courseId)
{
    // Lấy fullname từ claim (đang dùng ở dashboard)
    var fullName = User.FindFirst("FullName")?.Value?.Trim();

    // Lấy tất cả grades
    var grades = await _gradeService.GetAllAsync();

    // Lọc đúng student
    if (string.IsNullOrWhiteSpace(fullName))
        return View(new List<BusinessLogic.DTOs.Responses.GradeResponse>());

    var myGrades = grades
        .Where(g => (g.StudentName ?? "").Trim().Equals(fullName, StringComparison.OrdinalIgnoreCase))
        .ToList();

    // ✅ Dropdown course chỉ gồm môn mà student này có trong myGrades
    ViewBag.Courses = myGrades
        .Where(g => g.CourseId > 0)
        .GroupBy(g => new { g.CourseId, g.CourseName })
        .Select(x => new SelectListItem
        {
            Value = x.Key.CourseId.ToString(),
            Text = x.Key.CourseName
        })
        .OrderBy(x => x.Text)
        .ToList();

    // giữ lựa chọn
    ViewBag.SelectedCourseId = courseId;

    // ✅ Filter theo course (nếu chọn)
    if (courseId.HasValue && courseId.Value > 0)
    {
        myGrades = myGrades.Where(g => g.CourseId == courseId.Value).ToList();
    }

    return View(myGrades);
}

}
