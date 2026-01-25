using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Requests
{
    public class CreateCourseRequest
    {
        [Required(ErrorMessage = "Course code is required")]
        [StringLength(20, ErrorMessage = "Course code cannot exceed 20 characters")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course name is required")]
        [StringLength(200, ErrorMessage = "Course name cannot exceed 200 characters")]
        public string CourseName { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10")]
        public int? Credits { get; set; }

        [StringLength(20, ErrorMessage = "Semester cannot exceed 20 characters")]
        public string? Semester { get; set; }

        public int? TeacherId { get; set; }
    }
}
