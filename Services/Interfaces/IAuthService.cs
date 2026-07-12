using BTLWEB.ViewModels;

namespace BTLWEB.Services.Interfaces;

public interface IAuthService
{
    Task<OperationResult> LoginAsync(LoginViewModel model, string? ipAddress, string? userAgent);
    Task<OperationResult> RegisterAsync(RegisterViewModel model);
    Task SignOutAsync();
    Task<OperationResult<string?>> CreatePasswordResetTokenAsync(ForgotPasswordViewModel model, string? ipAddress);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordViewModel model);
}
