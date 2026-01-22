namespace BusinessLogic.DTOs.Responses;

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
}
