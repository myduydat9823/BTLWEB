using System.Security.Claims;
using BTLWEB.Common;
using BTLWEB.Models;
using BTLWEB.Repositories.Interfaces;
using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace BTLWEB.Services;

public class AuthService : IAuthService
{
    private const int MaxFailedLoginCount = 3;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(10);

    private readonly IAuthRepository _authRepository;
    private readonly ILoginLogRepository _loginLogRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAuthRepository authRepository,
        ILoginLogRepository loginLogRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IPasswordService passwordService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger)
    {
        _authRepository = authRepository;
        _loginLogRepository = loginLogRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _passwordService = passwordService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<OperationResult> LoginAsync(LoginViewModel model, string? ipAddress, string? userAgent)
    {
        var user = await _authRepository.GetByIdentifierAsync(model.Identifier);
        if (user is null)
        {
            await LogLoginAsync(null, model.Identifier, false, "Invalid credentials", ipAddress, userAgent);
            return OperationResult.Failure("Thông tin đăng nhập không đúng.");
        }

        if (user.IsDeleted || !user.IsActive)
        {
            await LogLoginAsync(user.UserId, model.Identifier, false, "Inactive or deleted account", ipAddress, userAgent);
            return OperationResult.Failure("Tài khoản đang bị vô hiệu hóa.");
        }

        if (user.LockoutEndUtc is not null && user.LockoutEndUtc > DateTime.UtcNow)
        {
            await LogLoginAsync(user.UserId, model.Identifier, false, "Account locked", ipAddress, userAgent);
            return OperationResult.Failure("Tài khoản đang bị khóa tạm thời. Vui lòng thử lại sau.");
        }

        var verification = _passwordService.VerifyPassword(user, user.PasswordHash, model.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            user.FailedLoginCount += 1;
            if (user.FailedLoginCount >= MaxFailedLoginCount)
            {
                user.LockoutEndUtc = DateTime.UtcNow.Add(LockoutDuration);
                user.FailedLoginCount = 0;
            }

            await _authRepository.UpdateUserAsync(user);
            await LogLoginAsync(user.UserId, model.Identifier, false, "Invalid password", ipAddress, userAgent);
            return OperationResult.Failure("Thông tin đăng nhập không đúng.");
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordService.HashPassword(user, model.Password);
        }

        user.FailedLoginCount = 0;
        user.LockoutEndUtc = null;
        user.LastLoginAtUtc = DateTime.UtcNow;
        await _authRepository.UpdateUserAsync(user);
        await SignInAsync(user, model.RememberMe);
        await LogLoginAsync(user.UserId, model.Identifier, true, null, ipAddress, userAgent);

        _logger.LogInformation("User {UserId} signed in.", user.UserId);
        return OperationResult.Success("Đăng nhập thành công.");
    }

    public async Task<OperationResult> RegisterAsync(RegisterViewModel model)
    {
        if (await _authRepository.UsernameExistsAsync(model.Username))
        {
            return OperationResult.Failure("Tên đăng nhập đã tồn tại.");
        }

        if (await _authRepository.EmailExistsAsync(model.Email))
        {
            return OperationResult.Failure("Email đã được sử dụng.");
        }

        var memberRole = await _authRepository.GetDefaultMemberRoleAsync();
        if (memberRole is null)
        {
            return OperationResult.Failure("Hệ thống chưa cấu hình role Member.");
        }

        var user = new User
        {
            Username = model.Username.Trim(),
            NormalizedUsername = model.Username.Trim().ToUpperInvariant(),
            Email = model.Email.Trim(),
            NormalizedEmail = model.Email.Trim().ToUpperInvariant(),
            FullName = model.FullName.Trim(),
            RoleId = memberRole.RoleId,
            Role = memberRole,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };
        user.PasswordHash = _passwordService.HashPassword(user, model.Password);

        await _authRepository.AddUserAsync(user);
        await SignInAsync(user, false);
        _logger.LogInformation("User {UserId} registered.", user.UserId);

        return OperationResult.Success("Đăng ký tài khoản thành công.");
    }

    public Task SignOutAsync()
    {
        return _httpContextAccessor.HttpContext?.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
            ?? Task.CompletedTask;
    }

    public async Task<OperationResult<string?>> CreatePasswordResetTokenAsync(ForgotPasswordViewModel model, string? ipAddress)
    {
        var user = await _authRepository.GetByEmailAsync(model.Email);
        if (user is null || user.IsDeleted || !user.IsActive)
        {
            return OperationResult<string?>.Success(null, "Nếu email tồn tại, hệ thống sẽ tạo liên kết đặt lại mật khẩu.");
        }

        var token = _passwordService.GenerateResetToken();
        await _passwordResetTokenRepository.AddAsync(new PasswordResetToken
        {
            UserId = user.UserId,
            TokenHash = _passwordService.HashToken(token),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
            CreatedIpAddress = ipAddress,
            CreatedAtUtc = DateTime.UtcNow
        });

        _logger.LogInformation("Password reset token created for user {UserId}.", user.UserId);
        return OperationResult<string?>.Success(token, "Đã tạo liên kết đặt lại mật khẩu.");
    }

    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordViewModel model)
    {
        var tokenHash = _passwordService.HashToken(model.Token);
        var resetToken = await _passwordResetTokenRepository.GetValidTokenAsync(tokenHash, model.Email);
        if (resetToken?.User is null)
        {
            return OperationResult.Failure("Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
        }

        var user = resetToken.User;
        user.PasswordHash = _passwordService.HashPassword(user, model.NewPassword);
        user.FailedLoginCount = 0;
        user.LockoutEndUtc = null;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _passwordResetTokenRepository.MarkUsedAsync(resetToken);
        await _authRepository.UpdateUserAsync(user);
        _logger.LogInformation("User {UserId} reset password.", user.UserId);

        return OperationResult.Success("Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.");
    }

    private async Task SignInAsync(User user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role?.RoleName ?? RoleNames.Member)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(8)
        };

        if (_httpContextAccessor.HttpContext is not null)
        {
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
        }
    }

    private Task LogLoginAsync(int? userId, string identifier, bool isSuccess, string? failureReason, string? ipAddress, string? userAgent)
    {
        return _loginLogRepository.AddAsync(new LoginLog
        {
            UserId = userId,
            Identifier = identifier,
            IsSuccess = isSuccess,
            FailureReason = failureReason,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAtUtc = DateTime.UtcNow
        });
    }
}
