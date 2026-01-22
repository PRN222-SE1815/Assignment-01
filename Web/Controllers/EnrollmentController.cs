using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        // TODO: Lấy studentId từ session/authentication sau khi implement login
        // Tạm thời hardcode để test
        private int GetCurrentStudentId()
        {
            // Lấy từ session nếu có
            var studentId = HttpContext.Session.GetInt32("StudentId");
            return studentId ?? 1; // Default = 1 để test
        }

        // GET: /Enrollment - Danh sách khóa học có sẵn để đăng ký
        public async Task<IActionResult> Index(string? search)
        {
            var studentId = GetCurrentStudentId();
            var courses = await _enrollmentService.GetAvailableCoursesAsync(studentId, search);
            
            ViewBag.SearchKeyword = search;
            return View(courses);
        }

        // GET: /Enrollment/MyCourses - Khóa học đã đăng ký của sinh viên
        public async Task<IActionResult> MyCourses()
        {
            var studentId = GetCurrentStudentId();
            var myCourses = await _enrollmentService.GetMyEnrolledCoursesAsync(studentId);
            return View(myCourses);
        }

        // POST: /Enrollment/Enroll - Đăng ký khóa học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            try
            {
                var studentId = GetCurrentStudentId();
                await _enrollmentService.EnrollCourseAsync(studentId, courseId);
                TempData["SuccessMessage"] = "Đăng ký khóa học thành công!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng ký khóa học!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Enrollment/Unenroll - Hủy đăng ký khóa học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unenroll(int courseId)
        {
            try
            {
                var studentId = GetCurrentStudentId();
                await _enrollmentService.UnenrollCourseAsync(studentId, courseId);
                TempData["SuccessMessage"] = "Hủy đăng ký khóa học thành công!";
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy đăng ký khóa học!";
            }

            return RedirectToAction(nameof(MyCourses));
        }
    }
}
