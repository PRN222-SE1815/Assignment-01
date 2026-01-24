using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Requests;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = null!;
}
