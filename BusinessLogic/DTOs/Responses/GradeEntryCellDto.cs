namespace BusinessLogic.DTOs.Response;

public class GradeEntryCellDto
{
    public int GradeEntryId { get; set; }
    public int GradeItemId { get; set; }
    public int EnrollmentId { get; set; }
    public decimal? Score { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
