using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

public partial class Teacher
{
    [Key]
    public int TeacherId { get; set; }

    public int UserId { get; set; }

    [StringLength(100)]
    public string? Department { get; set; }

    [InverseProperty("Teacher")]
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    [ForeignKey("UserId")]
    [InverseProperty("Teachers")]
    public virtual User User { get; set; } = null!;
}
