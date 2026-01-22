namespace BusinessLogic.Services.Interfaces;

public interface IForgotPasswordService
{
    Task RequestResetAsync(string email, string resetUrlBase);
    Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword);
}
