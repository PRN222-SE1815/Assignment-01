using BusinessLogic.DTOs.Request;
using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Web.Models.Admin;

namespace Web.Controllers.Admin;

[Authorize(Roles = nameof(UserRole.ADMIN))]
public class AdminUserController : Controller
{
    private readonly IUserService _userService;

    public AdminUserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? role, string? search, int page = 1, int pageSize = 10)
    {
        var result = await _userService.GetUsersAsync(new UserFilterDto
        {
            Role = role,
            Search = search,
            Page = page,
            PageSize = pageSize
        });

        var model = new UserListViewModel
        {
            Role = role,
            Search = search,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Users = result.Items.Select(user => new UserListItemViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            }).ToList()
        };

        return View("~/Views/Admin/UserManagement/Index.cshtml", model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var model = new UserDetailViewModel
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            StudentCode = user.StudentCode,
            ProgramId = user.ProgramId,
            CurrentSemesterId = user.CurrentSemesterId,
            TeacherCode = user.TeacherCode,
            Department = user.Department
        };

        return View("~/Views/Admin/UserManagement/Details.cshtml", model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("~/Views/Admin/UserManagement/Create.cshtml", new CreateUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/UserManagement/Create.cshtml", model);
        }

        if (string.IsNullOrWhiteSpace(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Role is required.");
            return View("~/Views/Admin/UserManagement/Create.cshtml", model);
        }

        if (!Enum.TryParse<UserRole>(model.Role.Trim(), true, out var role))
        {
            ModelState.AddModelError(nameof(model.Role), "Invalid role selected.");
            return View("~/Views/Admin/UserManagement/Create.cshtml", model);
        }

        try
        {
            if (role == UserRole.STUDENT)
            {
                if (string.IsNullOrWhiteSpace(model.StudentCode))
                {
                    ModelState.AddModelError(nameof(model.StudentCode), "Student code is required.");
                    return View(model);
                }

                var result = await _userService.CreateStudentAsync(new CreateStudentDto
                {
                    Username = model.Username,
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,
                    StudentCode = model.StudentCode.Trim(),
                    ProgramId = model.ProgramId,
                    CurrentSemesterId = model.CurrentSemesterId
                });

                TempData["SuccessMessage"] = $"Created student '{result.Username}'. Temporary password: {result.TemporaryPassword}";
                return RedirectToAction(nameof(Create));
            }

            if (role == UserRole.TEACHER)
            {
                if (string.IsNullOrWhiteSpace(model.TeacherCode))
                {
                    ModelState.AddModelError(nameof(model.TeacherCode), "Teacher code is required.");
                    return View("~/Views/Admin/UserManagement/Create.cshtml", model);
                }

                var result = await _userService.CreateTeacherAsync(new CreateTeacherDto
                {
                    Username = model.Username,
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,
                    TeacherCode = model.TeacherCode.Trim(),
                    Department = model.Department
                });

                TempData["SuccessMessage"] = $"Created teacher '{result.Username}'. Temporary password: {result.TemporaryPassword}";
                return RedirectToAction(nameof(Create));
            }

            if (role == UserRole.ADMIN)
            {
                var result = await _userService.CreateAdminAsync(new CreateUserDto
                {
                    Username = model.Username,
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password
                });

                TempData["SuccessMessage"] = $"Created admin '{result.Username}'. Temporary password: {result.TemporaryPassword}";
                return RedirectToAction(nameof(Create));
            }

            ModelState.AddModelError(nameof(model.Role), "Invalid role selected.");
            return View("~/Views/Admin/UserManagement/Create.cshtml", model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View("~/Views/Admin/UserManagement/Create.cshtml", model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var model = new UserEditViewModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Role = user.Role,
            FullName = user.FullName,
            Email = user.Email,
            IsActive = user.IsActive,
            StudentCode = user.StudentCode,
            TeacherCode = user.TeacherCode,
            Department = user.Department
        };

        return View("~/Views/Admin/UserManagement/Edit.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/UserManagement/Edit.cshtml", model);
        }

        try
        {
            await _userService.UpdateUserAsync(model.UserId, new UpdateUserDto
            {
                FullName = model.FullName,
                Email = model.Email,
                IsActive = model.IsActive,
                Password = model.Password,
                StudentCode = model.StudentCode,
                TeacherCode = model.TeacherCode,
                Department = model.Department
            });

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction(nameof(Edit), new { id = model.UserId });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View("~/Views/Admin/UserManagement/Edit.cshtml", model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _userService.ToggleUserStatusAsync(id);
            TempData["SuccessMessage"] = "User status updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
