using BusinessLogic.DTOs.Requests;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models.Admin;

namespace Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private const int PageSize = 10;
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Admin Dashboard";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UserManagement(string? search, int page = 1)
        {
            var paged = await _userService.GetAllAsync(search, page, PageSize);

            var vm = new UserManagementViewModel
            {
                PagedUsers = paged,
                Search = search,
                NewUser = new CreateUserRequest
                {
                    RoleId = 3,
                    IsActive = true
                }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser([Bind(Prefix = "NewUser")] CreateUserRequest request, string? search, int page = 1)
        {
            if (!ModelState.IsValid)
            {
                var paged = await _userService.GetAllAsync(search, page, PageSize);
                return View(nameof(UserManagement), new UserManagementViewModel { PagedUsers = paged, Search = search, NewUser = request });
            }

            try
            {
                await _userService.CreateAsync(request);
                TempData["Success"] = "Create user successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(UserManagement), new { search, page });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(int userId, UpdateUserRequest request, string? search, int page = 1)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid input.";
                return RedirectToAction(nameof(UserManagement), new { search, page });
            }

            try
            {
                await _userService.UpdateAsync(userId, request);
                TempData["Success"] = "Update user successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(UserManagement), new { search, page });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int userId, string? search, int page = 1)
        {
            try
            {
                var ok = await _userService.DeleteAsync(userId);
                TempData[ok ? "Success" : "Error"] = ok ? "Delete user successfully." : "User not found.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(UserManagement), new { search, page });
        }
    }
}
