using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class GradeInputModel
{
    [Required]
    public int CourseId { get; set; }

    [Required]
    public int EnrollmentId { get; set; }

    [Range(0, 10)]
    public decimal? Assignment { get; set; }

    [Range(0, 10)]
    public decimal? Midterm { get; set; }

    [Range(0, 10)]
    public decimal? Final { get; set; }

}
