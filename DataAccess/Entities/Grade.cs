using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Entities;

public partial class Grade
{
    [Key]
    public int GradeId { get; set; }

    public int EnrollmentId { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Assignment { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Midterm { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Final { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Total { get; set; }

    [ForeignKey("EnrollmentId")]
    [InverseProperty("Grades")]
    public virtual Enrollment Enrollment { get; set; } = null!;
}
