using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Requests;

public class UpdateUserRequest
{
    [StringLength(100)]
    public string? FullName { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    public bool? IsActive { get; set; }

    public int? RoleId { get; set; }

    [StringLength(100)]
    public string? NewPassword { get; set; }
}
