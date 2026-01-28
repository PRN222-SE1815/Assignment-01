namespace BusinessLogic.DTOs.Response;

public class StatsDto
{
    public IReadOnlyList<int> Histogram { get; set; } = Array.Empty<int>();
    public int AboveCount { get; set; }
    public int BelowCount { get; set; }
    public int NotGradedCount { get; set; }
}
