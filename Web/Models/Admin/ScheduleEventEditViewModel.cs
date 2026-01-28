using System.ComponentModel.DataAnnotations;

namespace Web.Models.Admin;

public class ScheduleEventEditViewModel
{
    [Required]
    public long ScheduleEventId { get; set; }

    [Required]
    public int ClassSectionId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime StartAtUtc { get; set; }

    [Required]
    public DateTime EndAtUtc { get; set; }

    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(500)]
    public string? OnlineUrl { get; set; }

    public int? TeacherId { get; set; }

    public string Status { get; set; } = string.Empty;

    public int? RecurrenceId { get; set; }

    [StringLength(500)]
    public string? RecurrenceRule { get; set; }

    public DateOnly? RecurrenceStartDate { get; set; }

    public DateOnly? RecurrenceEndDate { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}
