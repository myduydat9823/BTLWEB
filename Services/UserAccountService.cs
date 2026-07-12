using BTLWEB.Common;
using BTLWEB.Repositories.Interfaces;
using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace BTLWEB.Services;

public class UserAccountService : IUserAccountService
{
    private const int DefaultPageSize = 10;

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordService _passwordService;
    private readonly IDataEncryptionService _dataEncryptionService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<UserAccountService> _logger;

    public UserAccountService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordService passwordService,
        IDataEncryptionService dataEncryptionService,
        IWebHostEnvironment webHostEnvironment,
        ILogger<UserAccountService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordService = passwordService;
        _dataEncryptionService = dataEncryptionService;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    public async Task<OperationResult<ProfileViewModel>> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdWithRoleAsync(userId);
        if (user is null)
        {
            return OperationResult<ProfileViewModel>.Failure("Không tìm thấy tài khoản.");
        }

        return OperationResult<ProfileViewModel>.Success(new ProfileViewModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            RoleName = user.Role?.RoleName ?? string.Empty,
            RoleDisplayName = user.Role?.DisplayName ?? string.Empty,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Phone = _dataEncryptionService.Decrypt(user.PhoneEncrypted),
            Address = _dataEncryptionService.Decrypt(user.AddressEncrypted),
            DateOfBirth = _dataEncryptionService.DecryptDate(user.DateOfBirthEncrypted),
            CreatedAtUtc = user.CreatedAtUtc,
            LastLoginAtUtc = user.LastLoginAtUtc
        });
    }

    public async Task<OperationResult<EditProfileViewModel>> GetEditProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return OperationResult<EditProfileViewModel>.Failure("Không tìm thấy tài khoản.");
        }

        return OperationResult<EditProfileViewModel>.Success(new EditProfileViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Phone = _dataEncryptionService.Decrypt(user.PhoneEncrypted),
            Address = _dataEncryptionService.Decrypt(user.AddressEncrypted),
            DateOfBirth = _dataEncryptionService.DecryptDate(user.DateOfBirthEncrypted)
        });
    }

    public async Task<OperationResult> UpdateProfileAsync(int userId, EditProfileViewModel model)
    {
        if (userId != model.UserId)
        {
            return OperationResult.Failure("Bạn không có quyền sửa hồ sơ này.");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return OperationResult.Failure("Không tìm thấy tài khoản.");
        }

        user.FullName = model.FullName.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(model.AvatarUrl) ? null : model.AvatarUrl.Trim();
        var avatarResult = await SaveAvatarAsync(model.AvatarFile);
        if (!avatarResult.Succeeded)
        {
            return avatarResult;
        }

        if (!string.IsNullOrWhiteSpace(avatarResult.Message))
        {
            user.AvatarUrl = avatarResult.Message;
        }

        user.Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim();
        user.PhoneEncrypted = _dataEncryptionService.Encrypt(model.Phone);
        user.AddressEncrypted = _dataEncryptionService.Encrypt(model.Address);
        user.DateOfBirthEncrypted = _dataEncryptionService.EncryptDate(model.DateOfBirth);
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _userRepository.UpdateUserAsync(user);
        return OperationResult.Success("Đã cập nhật hồ sơ.");
    }

    public async Task<OperationResult> ChangePasswordAsync(int userId, ChangePasswordViewModel model)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return OperationResult.Failure("Không tìm thấy tài khoản.");
        }

        var verification = _passwordService.VerifyPassword(user, user.PasswordHash, model.CurrentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            return OperationResult.Failure("Mật khẩu hiện tại không đúng.");
        }

        user.PasswordHash = _passwordService.HashPassword(user, model.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user);

        return OperationResult.Success("Đã đổi mật khẩu.");
    }

    public async Task<UserListViewModel> GetUserListAsync(string? search, string? role, string? status, int page)
    {
        return new UserListViewModel
        {
            Search = search,
            Role = role,
            Status = status,
            Roles = await _roleRepository.GetRoleOptionsAsync(),
            Users = await _userRepository.GetUsersAsync(search, role, status, page, DefaultPageSize)
        };
    }

    public async Task<OperationResult<UserDetailsViewModel>> GetUserDetailsAsync(int userId)
    {
        var details = await _userRepository.GetUserDetailsAsync(
            userId,
            _dataEncryptionService.Decrypt,
            _dataEncryptionService.DecryptDate);

        return details is null
            ? OperationResult<UserDetailsViewModel>.Failure("Không tìm thấy tài khoản.")
            : OperationResult<UserDetailsViewModel>.Success(details);
    }

    public async Task<OperationResult<ChangeUserRoleViewModel>> GetChangeRoleAsync(int userId)
    {
        var user = await _userRepository.GetByIdWithRoleAsync(userId);
        if (user is null)
        {
            return OperationResult<ChangeUserRoleViewModel>.Failure("Không tìm thấy tài khoản.");
        }

        return OperationResult<ChangeUserRoleViewModel>.Success(new ChangeUserRoleViewModel
        {
            UserId = user.UserId,
            Username = user.Username,
            CurrentRoleName = user.Role?.RoleName ?? string.Empty,
            NewRoleId = user.RoleId,
            Roles = await _roleRepository.GetRoleOptionsAsync()
        });
    }

    public async Task<OperationResult> ChangeRoleAsync(ChangeUserRoleViewModel model, int? changedByUserId)
    {
        var user = await _userRepository.GetByIdWithRoleAsync(model.UserId);
        if (user is null)
        {
            return OperationResult.Failure("Không tìm thấy tài khoản.");
        }

        var newRole = await _roleRepository.GetByIdAsync(model.NewRoleId);
        if (newRole is null)
        {
            return OperationResult.Failure("Role mới không hợp lệ.");
        }

        if (user.Role?.RoleName == RoleNames.Admin
            && newRole.RoleName != RoleNames.Admin
            && !await _userRepository.HasAnotherActiveAdminAsync(user.UserId))
        {
            return OperationResult.Failure("Không thể thu hồi quyền Admin cuối cùng.");
        }

        await _userRepository.ChangeRoleAsync(user.UserId, newRole.RoleId, changedByUserId, model.Note);
        _logger.LogInformation("User {UserId} role changed to {RoleName} by {ChangedByUserId}.", user.UserId, newRole.RoleName, changedByUserId);
        return OperationResult.Success("Đã cập nhật role.");
    }

    public async Task<OperationResult> SetActiveAsync(int userId, bool isActive, int? currentUserId)
    {
        if (currentUserId == userId && !isActive)
        {
            return OperationResult.Failure("Bạn không thể tự khóa tài khoản đang đăng nhập.");
        }

        var user = await _userRepository.GetByIdWithRoleAsync(userId);
        if (user is null)
        {
            return OperationResult.Failure("Không tìm thấy tài khoản.");
        }

        if (!isActive
            && user.Role?.RoleName == RoleNames.Admin
            && !await _userRepository.HasAnotherActiveAdminAsync(user.UserId))
        {
            return OperationResult.Failure("Không thể khóa Admin cuối cùng.");
        }

        user.IsActive = isActive;
        user.LockoutEndUtc = isActive ? null : DateTime.UtcNow.AddYears(100);
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user);

        return OperationResult.Success(isActive ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.");
    }

    public async Task<OperationResult> SoftDeleteAsync(int userId, int? currentUserId)
    {
        if (currentUserId == userId)
        {
            return OperationResult.Failure("Bạn không thể tự xóa tài khoản đang đăng nhập.");
        }

        var user = await _userRepository.GetByIdWithRoleAsync(userId);
        if (user is null)
        {
            return OperationResult.Failure("Không tìm thấy tài khoản.");
        }

        if (user.Role?.RoleName == RoleNames.Admin && !await _userRepository.HasAnotherActiveAdminAsync(user.UserId))
        {
            return OperationResult.Failure("Không thể xóa Admin cuối cùng.");
        }

        user.IsDeleted = true;
        user.DeletedAtUtc = DateTime.UtcNow;
        user.IsActive = false;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user);

        return OperationResult.Success("Đã xóa mềm tài khoản.");
    }

    public async Task<OperationResult> RestoreAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return OperationResult.Failure("Không tìm thấy tài khoản.");
        }

        user.IsDeleted = false;
        user.DeletedAtUtc = null;
        user.IsActive = true;
        user.LockoutEndUtc = null;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user);

        return OperationResult.Success("Đã khôi phục tài khoản.");
    }

    private async Task<OperationResult> SaveAvatarAsync(IFormFile? avatarFile)
    {
        if (avatarFile is null || avatarFile.Length == 0)
        {
            return OperationResult.Success();
        }

        if (avatarFile.Length > 2 * 1024 * 1024)
        {
            return OperationResult.Failure("Ảnh đại diện tối đa 2MB.");
        }

        var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension))
        {
            return OperationResult.Failure("Ảnh đại diện chỉ chấp nhận JPG, PNG hoặc WEBP.");
        }

        var uploadRoot = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(uploadRoot);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadRoot, fileName);

        await using var stream = File.Create(physicalPath);
        await avatarFile.CopyToAsync(stream);

        return OperationResult.Success($"/uploads/avatars/{fileName}");
    }
}
