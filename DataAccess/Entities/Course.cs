using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

[Index("CourseCode", Name = "UQ__Courses__FC00E0000446175C", IsUnique = true)]
public partial class Course
{
    [Key]
    public int CourseId { get; set; }

    [StringLength(20)]
    public string CourseCode { get; set; } = null!;

    [StringLength(200)]
    public string CourseName { get; set; } = null!;

    public int? Credits { get; set; }

    [StringLength(20)]
    public string? Semester { get; set; }

    public int? TeacherId { get; set; }

    [InverseProperty("Course")]
    public virtual ICollection<CourseSchedule> CourseSchedules { get; set; } = new List<CourseSchedule>();

    [InverseProperty("Course")]
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    [ForeignKey("TeacherId")]
    [InverseProperty("Courses")]
    public virtual Teacher? Teacher { get; set; }
}
