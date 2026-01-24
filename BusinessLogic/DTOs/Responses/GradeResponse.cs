namespace BusinessLogic.DTOs.Responses;

public class GradeResponse
{
    public int GradeId { get; set; }
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }

    public string StudentName { get; set; } = "";
    public string CourseName { get; set; } = "";

    public decimal? Assignment { get; set; }
    public decimal? Midterm { get; set; }
    public decimal? Final { get; set; }
    public decimal? Total { get; set; }
}
