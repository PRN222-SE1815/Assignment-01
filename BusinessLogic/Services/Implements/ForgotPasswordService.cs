using System.Globalization;
using BusinessLogic.Services.Interfaces;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace BusinessLogic.Services.Implements;

public class ForgotPasswordService : IForgotPasswordService
{
    private const string Purpose = "ForgotPasswordToken_v1";
    private static readonly TimeSpan TokenTtl = TimeSpan.FromMinutes(15);

    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IDataProtector _protector;

    public ForgotPasswordService(
        IUserRepository userRepository,
        IEmailService emailService,
        IDataProtectionProvider dataProtectionProvider)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    public async Task RequestResetAsync(string email, string resetUrlBase)
    {
        // Always behave the same to prevent account enumeration.
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var normalizedEmail = email.Trim();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (user == null)
        {
            return;
        }

        // Token payload includes userId + issued ticks + snapshot of password hash
        // Snapshot invalidates token automatically after password change.
        var issuedUtc = DateTimeOffset.UtcNow;
        var payload = string.Join('|',
            user.UserId.ToString(CultureInfo.InvariantCulture),
            issuedUtc.UtcTicks.ToString(CultureInfo.InvariantCulture),
            user.PasswordHash);

        var token = _protector.Protect(payload);
        var encodedToken = Uri.EscapeDataString(token);

        var resetLink = $"{resetUrlBase}?token={encodedToken}";

        var subject = "Reset your password";
        var html = $"""
            <p>Hello {System.Net.WebUtility.HtmlEncode(user.FullName)},</p>
            <p>We received a password reset request for your account.</p>
            <p>This link will expire in 15 minutes.</p>
            <p><a href='{resetLink}'>Click here to reset your password</a></p>
            <p>If you did not request a reset, you can ignore this email.</p>
            """;

        await _emailService.SendAsync(user.Email!, subject, html);
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return (false, "Invalid token.");
        }

        string unprotected;
        try
        {
            unprotected = _protector.Unprotect(token);
        }
        catch
        {
            return (false, "Invalid or expired token.");
        }

        var parts = unprotected.Split('|');
        if (parts.Length != 3)
        {
            return (false, "Invalid token.");
        }

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var userId))
        {
            return (false, "Invalid token.");
        }

        if (!long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var issuedTicks))
        {
            return (false, "Invalid token.");
        }

        var passwordHashSnapshot = parts[2];
        var issuedUtc = new DateTimeOffset(issuedTicks, TimeSpan.Zero);

        if (DateTimeOffset.UtcNow - issuedUtc > TokenTtl)
        {
            return (false, "Token expired.");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return (false, "Invalid token.");
        }

        if (!string.Equals(user.PasswordHash, passwordHashSnapshot, StringComparison.Ordinal))
        {
            return (false, "Token expired.");
        }

        if (user.IsActive != true)
        {
            return (false, "User is inactive.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);

        return (true, "Password reset successfully.");
    }
}
