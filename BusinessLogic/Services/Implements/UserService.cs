﻿using BusinessLogic.DTOs.Requests;
using BusinessLogic.DTOs.Responses;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories.Interfaces;

namespace BusinessLogic.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<PagedResult<UserDto>> GetAllAsync(string? search, int pageNumber, int pageSize)
        {
            var users = await _userRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                users = users
                    .Where(u =>
                        u.Username.Contains(s) ||
                        u.FullName.Contains(s) ||
                        (u.Email != null && u.Email.Contains(s)))
                    .ToList();
            }

            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var total = users.Count;
            var items = users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();

            return new PagedResult<UserDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        public async Task<UserDto?> GetByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto> CreateAsync(CreateUserRequest request)
        {
            if (await _userRepository.UsernameExistsAsync(request.Username))
            {
                throw new InvalidOperationException("Username already exists.");
            }

            if (!string.IsNullOrWhiteSpace(request.Email) && await _userRepository.EmailExistsAsync(request.Email))
            {
                throw new InvalidOperationException("Email already exists.");
            }

            var entity = new User
            {
                Username = request.Username.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName.Trim(),
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                IsActive = request.IsActive,
                RoleId = request.RoleId,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _userRepository.CreateAsync(entity);
            return MapToDto(created);
        }

        public async Task<UserDto> UpdateAsync(int userId, UpdateUserRequest request)
        {
            var existing = await _userRepository.GetByIdAsync(userId);
            if (existing == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var email = request.Email.Trim();
                var emailExists = await _userRepository.EmailExistsAsync(email);
                if (emailExists && !string.Equals(existing.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Email already exists.");
                }

                existing.Email = email;
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                existing.FullName = request.FullName.Trim();
            }

            if (request.IsActive.HasValue)
            {
                existing.IsActive = request.IsActive;
            }

            if (request.RoleId.HasValue)
            {
                existing.RoleId = request.RoleId;
            }

            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            }

            var updated = await _userRepository.UpdateAsync(existing);
            return MapToDto(updated);
        }

        public Task<bool> DeleteAsync(int userId)
        {
            return _userRepository.DeleteAsync(userId);
        }

        public async Task<List<UserSearchResponse>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<UserSearchResponse>();
            }

            var users = await _userRepository.SearchUsersAsync(searchTerm);
            
            return users.Select(u => new UserSearchResponse
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Username = u.Username,
                RoleName = u.Role?.RoleName ?? "Unknown"
            }).ToList();
        }

        private static UserDto MapToDto(User u)
        {
            return new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                IsActive = u.IsActive,
                RoleId = u.RoleId,
                RoleName = u.Role?.RoleName
            };
        }
    }
}
