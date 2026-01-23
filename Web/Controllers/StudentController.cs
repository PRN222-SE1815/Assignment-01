﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repositories.Interfaces;
using System.Security.Claims;

namespace Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IEnrollmentServiceForChat _enrollmentServiceForChat;
        private readonly IStudentRepository _studentRepository;

        public StudentController(
            IEnrollmentServiceForChat enrollmentServiceForChat,
            IStudentRepository studentRepository)
        {
            _enrollmentServiceForChat = enrollmentServiceForChat;
            _studentRepository = studentRepository;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Student Dashboard";
            return View();
        }

        /// <summary>
        /// Enroll student to course and auto-create course conversation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollCourse(int courseId)
        {
            try
            {
                var studentId = await GetCurrentStudentIdAsync();
                if (studentId == 0)
                {
                    TempData["Error"] = "Student not found.";
                    return RedirectToAction("Index");
                }

                await _enrollmentServiceForChat.EnrollStudentToCourseAsync(studentId, courseId);
                
                TempData["Success"] = "Enrolled successfully! You can now access the course chat.";
                return RedirectToAction("Index", "Chat");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Get my enrollments
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MyEnrollments()
        {
            try
            {
                var studentId = await GetCurrentStudentIdAsync();
                if (studentId == 0)
                {
                    TempData["Error"] = "Student not found.";
                    return RedirectToAction("Index");
                }

                var enrollments = await _enrollmentServiceForChat.GetStudentEnrollmentsAsync(studentId);
                return View(enrollments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        private async Task<int> GetCurrentStudentIdAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId) || userId == 0)
            {
                return 0;
            }

            var student = await _studentRepository.GetStudentByUserIdAsync(userId);
            return student?.StudentId ?? 0;
        }
    }
}
