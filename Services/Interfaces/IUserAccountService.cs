using BTLWEB.ViewModels;

namespace BTLWEB.Services.Interfaces;

public interface IUserAccountService
{
    Task<OperationResult<ProfileViewModel>> GetProfileAsync(int userId);
    Task<OperationResult<EditProfileViewModel>> GetEditProfileAsync(int userId);
    Task<OperationResult> UpdateProfileAsync(int userId, EditProfileViewModel model);
    Task<OperationResult> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
    Task<UserListViewModel> GetUserListAsync(string? search, string? role, string? status, int page);
    Task<OperationResult<UserDetailsViewModel>> GetUserDetailsAsync(int userId);
    Task<OperationResult<ChangeUserRoleViewModel>> GetChangeRoleAsync(int userId);
    Task<OperationResult> ChangeRoleAsync(ChangeUserRoleViewModel model, int? changedByUserId);
    Task<OperationResult> SetActiveAsync(int userId, bool isActive, int? currentUserId);
    Task<OperationResult> SoftDeleteAsync(int userId, int? currentUserId);
    Task<OperationResult> RestoreAsync(int userId);
}
