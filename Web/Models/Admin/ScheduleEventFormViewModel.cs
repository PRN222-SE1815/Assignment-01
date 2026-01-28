using System.ComponentModel.DataAnnotations;

namespace Web.Models.Admin;

public class ScheduleEventFormViewModel
{
    [Required]
    public int ClassSectionId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime StartAtUtc { get; set; }

    [Required]
    public DateTime EndAtUtc { get; set; }

    public string Timezone { get; set; } = "Asia/Ho_Chi_Minh";
    public string? Location { get; set; }
    public string? OnlineUrl { get; set; }
    public int? TeacherId { get; set; }
    public string? RecurrenceRule { get; set; }
    public DateOnly? RecurrenceStartDate { get; set; }
    public DateOnly? RecurrenceEndDate { get; set; }
}
