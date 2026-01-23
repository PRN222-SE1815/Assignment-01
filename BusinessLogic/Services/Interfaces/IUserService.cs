using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;

namespace BusinessLogic.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResult<UserDto>> GetAllAsync(string? search, int pageNumber, int pageSize);
        Task<UserDto?> GetByIdAsync(int userId);
        Task<UserDto> CreateAsync(CreateUserRequest request);
        Task<UserDto> UpdateAsync(int userId, UpdateUserRequest request);
        Task<bool> DeleteAsync(int userId);
        Task<List<UserSearchResponse>> SearchUsersAsync(string searchTerm);
    }
}
