using System.ComponentModel.DataAnnotations;

namespace Web.Models.Admin;

public class ScheduleOverrideViewModel
{
    [Required]
    public long ScheduleEventId { get; set; }

    [Required]
    public int RecurrenceId { get; set; }

    [Required]
    public DateOnly OriginalDate { get; set; }

    [Required]
    public string OverrideType { get; set; } = string.Empty;

    public DateTime? NewStartAtUtc { get; set; }
    public DateTime? NewEndAtUtc { get; set; }
    public string? NewLocation { get; set; }
    public int? NewTeacherId { get; set; }
    public string? Reason { get; set; }
}
