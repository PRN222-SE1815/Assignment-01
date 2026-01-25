using BusinessLogic.DTOs.Requests;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Controllers
{
    public class AdminCourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ITeacherRepository _teacherRepository;

        public AdminCourseController(
            ICourseService courseService,
            ITeacherRepository teacherRepository)
        {
            _courseService = courseService;
            _teacherRepository = teacherRepository;
        }

        // GET: AdminCourse
        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return View(courses);
        }

        // GET: AdminCourse/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // GET: AdminCourse/Create
        public async Task<IActionResult> Create()
        {
            await PopulateTeachersDropdownAsync();
            return View();
        }

        // POST: AdminCourse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseRequest request)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _courseService.CreateCourseAsync(request);
                    TempData["SuccessMessage"] = "Course created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                }
            }

            await PopulateTeachersDropdownAsync(request.TeacherId);
            return View(request);
        }

        // GET: AdminCourse/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var updateRequest = new UpdateCourseRequest
            {
                CourseId = course.CourseId,
                CourseCode = course.CourseCode,
                CourseName = course.CourseName,
                Credits = course.Credits,
                Semester = course.Semester,
                TeacherId = course.TeacherId
            };

            await PopulateTeachersDropdownAsync(course.TeacherId);
            return View(updateRequest);
        }

        // POST: AdminCourse/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateCourseRequest request)
        {
            if (id != request.CourseId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _courseService.UpdateCourseAsync(request);
                    TempData["SuccessMessage"] = "Course updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred: " + ex.Message);
                }
            }

            await PopulateTeachersDropdownAsync(request.TeacherId);
            return View(request);
        }

        // GET: AdminCourse/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: AdminCourse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _courseService.DeleteCourseAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Course deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Course not found.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // API for checking course code uniqueness (for client-side validation)
        [HttpGet]
        public async Task<JsonResult> CheckCourseCode(string courseCode, int? courseId)
        {
            var exists = await _courseService.CourseCodeExistsAsync(courseCode, courseId);
            return Json(!exists); // Return true if available (not exists)
        }

        private async Task PopulateTeachersDropdownAsync(int? selectedTeacherId = null)
        {
            var teachers = await _teacherRepository.GetAllTeachersAsync();
            ViewBag.Teachers = new SelectList(
                teachers.Select(t => new 
                { 
                    t.TeacherId, 
                    DisplayName = $"{t.User.FullName} - {t.Department ?? "N/A"}" 
                }),
                "TeacherId",
                "DisplayName",
                selectedTeacherId);
        }
    }
}
