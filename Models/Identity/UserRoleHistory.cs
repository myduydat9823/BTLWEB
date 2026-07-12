namespace BTLWEB.Models;

public class UserRoleHistory
{
    public int UserRoleHistoryId { get; set; }
    public int UserId { get; set; }
    public int? OldRoleId { get; set; }
    public int NewRoleId { get; set; }
    public int? ChangedByUserId { get; set; }
    public string? Note { get; set; }
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Role? OldRole { get; set; }
    public Role? NewRole { get; set; }
    public User? ChangedByUser { get; set; }
}
