namespace BusinessLogic.DTOs.Responses
{
    public class MyEnrolledCourseResponse
    {
        public int EnrollmentId { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int? Credits { get; set; }
        public string? Semester { get; set; }
        public string? TeacherName { get; set; }
        public DateOnly? EnrollDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
