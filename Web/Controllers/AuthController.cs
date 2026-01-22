using System.Security.Claims;
using BusinessLogic.DTOs.Requests;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models.Auth;

namespace Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IForgotPasswordService _forgotPasswordService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IForgotPasswordService forgotPasswordService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _forgotPasswordService = forgotPasswordService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleHome();
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var loginRequest = new LoginRequest
                {
                    Username = model.Username,
                    Password = model.Password
                };

                var result = await _authService.LoginAsync(loginRequest);

                if (!result.Success || result.User == null)
                {
                    ModelState.AddModelError(string.Empty, result.Message ?? "Login failed");
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, result.User.UserId.ToString()),
                    new Claim(ClaimTypes.Name, result.User.Username),
                    new Claim(ClaimTypes.Email, result.User.Email ?? string.Empty),
                    new Claim("FullName", result.User.FullName),
                    new Claim(ClaimTypes.Role, result.User.RoleName ?? "Student")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("User {Username} logged in successfully with role {Role}", 
                    result.User.Username, result.User.RoleName);

                // Redirect based on role or returnUrl
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToRoleHome(result.User.RoleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", model.Username);
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User {Username} logged out", User.Identity?.Name);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            ViewData["Title"] = "Forgot Password";
            return View(new ForgotPasswordRequest());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                var resetUrlBase = $"{Request.Scheme}://{Request.Host}/Auth/ResetPassword";
                await _forgotPasswordService.RequestResetAsync(request.Email, resetUrlBase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting password reset for email {Email}", request.Email);
            }

            // Always show the same message to avoid account enumeration
            TempData["Success"] = "If the email exists, a reset link has been sent.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token)
        {
            ViewData["Title"] = "Reset Password";
            return View(new ResetPasswordRequest { Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var (success, message) = await _forgotPasswordService.ResetPasswordAsync(request.Token, request.NewPassword);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(request);
            }

            TempData["Success"] = "Password has been reset. Please login.";
            return RedirectToAction(nameof(Login));
        }

        private IActionResult RedirectToRoleHome(string? roleName = null)
        {
            var role = roleName ?? User.FindFirstValue(ClaimTypes.Role);

            return role switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Teacher" => RedirectToAction("Index", "Teacher"),
                "Student" => RedirectToAction("Index", "Student"),
                _ => RedirectToAction(nameof(Login))
            };
        }
    }
}
