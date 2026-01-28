using System.ComponentModel.DataAnnotations;

namespace Web.Models.Admin;

public sealed class UserEditViewModel
{
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    public bool IsActive { get; set; }

    [StringLength(200)]
    public string? Password { get; set; }

    [StringLength(50)]
    public string? StudentCode { get; set; }

    [StringLength(50)]
    public string? TeacherCode { get; set; }

    [StringLength(200)]
    public string? Department { get; set; }
}
