using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Requests;

public class GradeUpsertRequest
{
    [Required]
    public int EnrollmentId { get; set; }

    [Range(typeof(decimal), "0", "10")]
    public decimal? Assignment { get; set; }

    [Range(typeof(decimal), "0", "10")]
    public decimal? Midterm { get; set; }

    [Range(typeof(decimal), "0", "10")]
    public decimal? Final { get; set; }
}
