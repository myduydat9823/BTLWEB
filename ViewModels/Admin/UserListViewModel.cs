namespace BTLWEB.ViewModels;

public class UserListViewModel
{
    public PagedResult<UserListItemViewModel> Users { get; set; } = new();
    public IReadOnlyList<RoleOptionViewModel> Roles { get; set; } = [];
    public string? Search { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }
}
