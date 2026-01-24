namespace BusinessLogic.DTOs.Responses
{
    public class CourseResponse
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int? Credits { get; set; }
        public string? Semester { get; set; }
        public string? TeacherName { get; set; }
        public string? Department { get; set; }
        public int EnrolledCount { get; set; }
        public bool IsEnrolled { get; set; }
    }
}
