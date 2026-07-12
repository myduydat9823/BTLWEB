namespace BTLWEB.Services.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    string? RoleName { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string roleName);
}
