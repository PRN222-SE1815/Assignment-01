using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Requests
{
    public class CourseEnrollRequest
    {
        [Required(ErrorMessage = "Course is required")]
        public int CourseId { get; set; }
    }
}
