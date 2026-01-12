using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

public partial class Student
{
    [Key]
    public int StudentId { get; set; }

    public int UserId { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [StringLength(100)]
    public string? Major { get; set; }

    public int? EnrollmentYear { get; set; }

    [InverseProperty("Student")]
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    [ForeignKey("UserId")]
    [InverseProperty("Students")]
    public virtual User User { get; set; } = null!;
}
