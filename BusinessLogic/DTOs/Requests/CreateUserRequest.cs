using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Requests;

public class CreateUserRequest
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = null!;

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [Required]
    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;
}
