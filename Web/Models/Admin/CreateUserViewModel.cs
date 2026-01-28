using System.ComponentModel.DataAnnotations;

namespace Web.Models.Admin;

public class CreateUserViewModel
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    [StringLength(200)]
    public string? Password { get; set; }

    [Required]
    public string Role { get; set; } = "STUDENT";

    [StringLength(50)]
    public string? StudentCode { get; set; }

    public int? ProgramId { get; set; }

    public int? CurrentSemesterId { get; set; }

    [StringLength(50)]
    public string? TeacherCode { get; set; }

    [StringLength(200)]
    public string? Department { get; set; }
}
