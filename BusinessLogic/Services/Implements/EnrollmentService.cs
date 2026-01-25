using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IStudentRepository _studentRepository;

        public EnrollmentService(
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            IConversationRepository conversationRepository,
            IStudentRepository studentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _conversationRepository = conversationRepository;
            _studentRepository = studentRepository;
        }

        public async Task<List<CourseResponse>> GetAvailableCoursesAsync(int studentId, string? searchKeyword, string? filter)
        {
            var allCourses = await _courseRepository.GetAllCoursesAsync();
            
            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                allCourses = allCourses
                    .Where(c => c.CourseCode.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) ||
                                c.CourseName.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) ||
                                (c.Teacher?.User?.FullName?.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            var result = new List<CourseResponse>();
            
            foreach (var course in allCourses)
            {
                var isEnrolled = await _enrollmentRepository.IsStudentEnrolledAsync(studentId, course.CourseId);
                var enrolledCount = await _enrollmentRepository.GetEnrolledCountByCourseAsync(course.CourseId);
                
                var courseResponse = new CourseResponse
                {
                    CourseId = course.CourseId,
                    CourseCode = course.CourseCode,
                    CourseName = course.CourseName,
                    Credits = course.Credits,
                    Semester = course.Semester,
                    TeacherName = course.Teacher?.User?.FullName,
                    Department = course.Teacher?.Department,
                    EnrolledCount = enrolledCount,
                    IsEnrolled = isEnrolled
                };

                // Áp dụng filter theo trạng thái đăng ký
                switch (filter?.ToLower())
                {
                    case "enrolled":
                        if (isEnrolled) result.Add(courseResponse);
                        break;
                    case "notenrolled":
                        if (!isEnrolled) result.Add(courseResponse);
                        break;
                    case "all":
                    default:
                        result.Add(courseResponse);
                        break;
                }
            }

            return result.OrderBy(c => c.CourseCode).ToList();
        }

        public async Task<List<MyEnrolledCourseResponse>> GetMyEnrolledCoursesAsync(int studentId)
        {
            var enrollments = await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(studentId);
            
            return enrollments.Select(e => new MyEnrolledCourseResponse
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                CourseCode = e.Course?.CourseCode ?? "Unknown",
                CourseName = e.Course?.CourseName ?? "Unknown",
                Credits = e.Course?.Credits,
                Semester = e.Course?.Semester,
                TeacherName = e.Course?.Teacher?.User?.FullName,
                EnrollDate = e.EnrollDate,
                Status = e.Status ?? "Unknown"
            }).ToList();
        }

        public async Task<bool> EnrollCourseAsync(int studentId, int courseId)
        {
            // Kiểm tra đã đăng ký Active chưa
            var isEnrolled = await _enrollmentRepository.IsStudentEnrolledAsync(studentId, courseId);
            if (isEnrolled)
            {
                throw new InvalidOperationException("Bạn đã đăng ký khóa học này rồi!");
            }

            // Kiểm tra xem có enrollment cũ bị Dropped không
            var existingEnrollment = await _enrollmentRepository.GetEnrollmentByStudentAndCourseAsync(studentId, courseId);
            
            if (existingEnrollment != null && existingEnrollment.Status == "Dropped")
            {
                // Nếu có enrollment đã bị Dropped, cập nhật lại thành Active
                await _enrollmentRepository.UpdateEnrollmentStatusAsync(existingEnrollment.EnrollmentId, "Active");
                
                // Thêm lại vào group chat
                await AddStudentToCourseConversationAsync(courseId, studentId);
                
                return true;
            }

            // Kiểm tra khóa học có tồn tại không
            var course = await _courseRepository.GetCourseByIdAsync(courseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Khóa học không tồn tại!");
            }

            // Tạo enrollment mới
            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollDate = DateOnly.FromDateTime(DateTime.Now),
                Status = "Active"
            };

            await _enrollmentRepository.CreateEnrollmentAsync(enrollment);

            // Thêm sinh viên vào group chat của khóa học (nếu có)
            await AddStudentToCourseConversationAsync(courseId, studentId);

            return true;
        }

        public async Task<bool> UnenrollCourseAsync(int studentId, int courseId)
        {
            var enrollment = await _enrollmentRepository.GetEnrollmentByStudentAndCourseAsync(studentId, courseId);
            if (enrollment == null || enrollment.Status != "Active")
            {
                throw new KeyNotFoundException("Bạn chưa đăng ký khóa học này hoặc đã hủy trước đó!");
            }

            // Cập nhật status thành "Dropped" thay vì xóa
            var result = await _enrollmentRepository.UpdateEnrollmentStatusAsync(enrollment.EnrollmentId, "Dropped");
            
            if (result)
            {
                // Xóa khỏi group chat của khóa học
                var student = await _studentRepository.GetStudentByIdAsync(studentId);
                if (student != null)
                {
                    await RemoveStudentFromCourseConversationAsync(courseId, student.UserId);
                }
            }

            return result;
        }

        private async Task AddStudentToCourseConversationAsync(int courseId, int studentId)
        {
            try
            {
                var conversation = await _conversationRepository.GetConversationByCourseIdAsync(courseId);
                if (conversation != null)
                {
                    var student = await _studentRepository.GetStudentByIdAsync(studentId);
                    if (student != null)
                    {
                        await _conversationRepository.AddParticipantAsync(conversation.ConversationId, student.UserId);
                    }
                }
            }
            catch
            {
                // Silent fail - conversation integration is optional
            }
        }

        private async Task RemoveStudentFromCourseConversationAsync(int courseId, int userId)
        {
            try
            {
                var conversation = await _conversationRepository.GetConversationByCourseIdAsync(courseId);
                if (conversation != null)
                {
                    await _conversationRepository.RemoveParticipantAsync(conversation.ConversationId, userId);
                }
            }
            catch
            {
                // Silent fail - conversation integration is optional
            }
        }
    }
}
