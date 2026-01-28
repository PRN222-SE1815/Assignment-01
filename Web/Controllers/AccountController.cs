using System.Security.Claims;
using BusinessLogic.Services.Interfaces;
using BusinessObject.Enum;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models.Account;

namespace Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(UserRole.ADMIN.ToString()))
            {
                return RedirectToAction("Index", "AdminHome");
            }
            else if (User.IsInRole(UserRole.TEACHER.ToString()))
            {
                return RedirectToAction("Index", "TeacherHome");
            }
            else if (User.IsInRole(UserRole.STUDENT.ToString()))
            {
                return RedirectToAction("Index", "StudentHome");
            }

            return RedirectToAction("AccessDenied", "Account");
        }

        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var authUser = await _authService.LoginAsync(model.Username, model.Password);
            if (authUser == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, authUser.UserId.ToString()),
                new Claim(ClaimTypes.Name, authUser.Username),
                new Claim(ClaimTypes.Role, authUser.Role)
            };

            if (authUser.StudentId.HasValue)
            {
                claims.Add(new Claim("StudentId", authUser.StudentId.Value.ToString()));
            }

            if (authUser.TeacherId.HasValue)
            {
                claims.Add(new Claim("TeacherId", authUser.TeacherId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (string.Equals(authUser.Role, UserRole.ADMIN.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "AdminHome");
            }
            else if (string.Equals(authUser.Role, UserRole.TEACHER.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "TeacherHome");
            }
            else if (string.Equals(authUser.Role, UserRole.STUDENT.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "StudentHome");
            }

            return RedirectToAction("AccessDenied", "Account");
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Unable to sign in. Please try again.");
            return View(model);
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
