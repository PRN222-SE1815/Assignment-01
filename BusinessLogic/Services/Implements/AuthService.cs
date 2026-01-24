using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                return new LoginResponse { Success = false, Message = "Invalid username or password." };
            }

            if (user.IsActive != true)
            {
                return new LoginResponse { Success = false, Message = "User is inactive." };
            }

            var ok = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!ok)
            {
                return new LoginResponse { Success = false, Message = "Invalid username or password." };
            }

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful.",
                User = new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    RoleId = user.RoleId,
                    RoleName = user.Role?.RoleName
                }
            };
        }
    }
}
