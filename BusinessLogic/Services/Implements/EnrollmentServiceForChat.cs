using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements
{
    /// <summary>
    /// Service for managing enrollments with auto course conversation creation
    /// </summary>
    public class EnrollmentServiceForChat : IEnrollmentServiceForChat
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseConversationService _courseConversationService;

        public EnrollmentServiceForChat(
            IEnrollmentRepository enrollmentRepository,
            ICourseConversationService courseConversationService)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseConversationService = courseConversationService;
        }

        public async Task<EnrollmentResponseForChat> EnrollStudentToCourseAsync(int studentId, int courseId)
        {
            // Check if already enrolled
            var isEnrolled = await _enrollmentRepository.IsStudentEnrolledAsync(studentId, courseId);
            if (isEnrolled)
            {
                throw new InvalidOperationException("Student is already enrolled in this course.");
            }

            // Create enrollment
            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollDate = DateOnly.FromDateTime(DateTime.Now),
                Status = "Active"
            };

            enrollment = await _enrollmentRepository.CreateEnrollmentAsync(enrollment);

            // ⭐ AUTO CREATE/SYNC COURSE CONVERSATION ⭐
            // Khi student enroll vào course → tự động tạo/sync conversation
            await _courseConversationService.GetOrCreateCourseConversationAsync(courseId);
            await _courseConversationService.SyncCourseParticipantsAsync(courseId);

            // Reload to get full data
            var enrollmentWithDetails = await _enrollmentRepository.GetEnrollmentByIdAsync(enrollment.EnrollmentId);

            return MapToResponse(enrollmentWithDetails!);
        }

        public async Task<List<EnrollmentResponseForChat>> GetStudentEnrollmentsAsync(int studentId)
        {
            var enrollments = await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(studentId);
            return enrollments.Select(MapToResponse).ToList();
        }

        public async Task<List<EnrollmentResponseForChat>> GetCourseEnrollmentsAsync(int courseId)
        {
            var enrollments = await _enrollmentRepository.GetEnrollmentsByCourseIdAsync(courseId);
            return enrollments.Select(MapToResponse).ToList();
        }

        private EnrollmentResponseForChat MapToResponse(Enrollment enrollment)
        {
            return new EnrollmentResponseForChat
            {
                EnrollmentId = enrollment.EnrollmentId,
                StudentId = enrollment.StudentId,
                StudentName = enrollment.Student?.User?.FullName ?? "Unknown",
                CourseId = enrollment.CourseId,
                CourseCode = enrollment.Course?.CourseCode ?? "",
                CourseName = enrollment.Course?.CourseName ?? "",
                EnrollDate = enrollment.EnrollDate,
                Status = enrollment.Status
            };
        }
    }
}

