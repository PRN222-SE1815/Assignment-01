namespace BusinessLogic.DTOs.Response;

public class StudentRowDto
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public decimal? FinalScore { get; set; }
}
