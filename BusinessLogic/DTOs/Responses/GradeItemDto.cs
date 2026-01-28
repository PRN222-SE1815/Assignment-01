namespace BusinessLogic.DTOs.Response;

public class GradeItemDto
{
    public int GradeItemId { get; set; }
    public int GradeBookId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public decimal? Weight { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
