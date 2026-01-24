using System;

namespace BusinessLogic.DTOs.Responses
{
    public class EnrollmentResponseForChat
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateOnly? EnrollDate { get; set; }
        public string? Status { get; set; }
    }
}

