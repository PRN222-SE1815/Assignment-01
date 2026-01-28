using System.ComponentModel.DataAnnotations;

namespace Web.Models.Account;

public class LoginViewModel
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
