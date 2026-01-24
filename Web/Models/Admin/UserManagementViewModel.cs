using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;

namespace Web.Models.Admin;

public class UserManagementViewModel
{
    public required PagedResult<UserDto> PagedUsers { get; set; }
    public string? Search { get; set; }

    public CreateUserRequest NewUser { get; set; } = new();
}
