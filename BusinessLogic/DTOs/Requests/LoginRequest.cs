using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Requests;

public class LoginRequest
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = null!;
}
