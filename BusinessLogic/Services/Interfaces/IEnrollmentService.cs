using BusinessLogic.DTOs.Responses;

namespace BusinessLogic.Services.Interfaces
{
    public interface IEnrollmentService
    {
        // Cho sinh viên xem danh sách khóa học có sẵn với filter
        Task<List<CourseResponse>> GetAvailableCoursesAsync(int studentId, string? searchKeyword, string? filter);
        
        // Cho sinh viên xem các khóa học đã đăng ký của mình
        Task<List<MyEnrolledCourseResponse>> GetMyEnrolledCoursesAsync(int studentId);
        
        // Sinh viên đăng ký khóa học
        Task<bool> EnrollCourseAsync(int studentId, int courseId);
        
        // Sinh viên hủy đăng ký khóa học
        Task<bool> UnenrollCourseAsync(int studentId, int courseId);
    }
}
