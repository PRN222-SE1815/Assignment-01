using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

public partial class CourseSchedule
{
    [Key]
    public int CourseScheduleId { get; set; }

    public int CourseId { get; set; }

    public byte DayOfWeek { get; set; }

    [Precision(0)]
    public TimeOnly StartTime { get; set; }

    [Precision(0)]
    public TimeOnly EndTime { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("CourseSchedules")]
    public virtual Course Course { get; set; } = null!;
}
