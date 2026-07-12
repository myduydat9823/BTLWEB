namespace BTLWEB.ViewModels;

public class UserRoleHistoryItemViewModel
{
    public string? OldRoleName { get; set; }
    public string NewRoleName { get; set; } = string.Empty;
    public string? ChangedByUsername { get; set; }
    public string? Note { get; set; }
    public DateTime ChangedAtUtc { get; set; }
}
