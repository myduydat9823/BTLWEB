using System.ComponentModel.DataAnnotations;

namespace BTLWEB.ViewModels;

public class ChangeUserRoleViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string CurrentRoleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn role mới.")]
    [Display(Name = "Role mới")]
    public int NewRoleId { get; set; }

    [StringLength(500)]
    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }

    public IReadOnlyList<RoleOptionViewModel> Roles { get; set; } = [];
}
