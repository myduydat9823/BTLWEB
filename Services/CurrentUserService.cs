using System.Security.Claims;
using BTLWEB.Services.Interfaces;

namespace BTLWEB.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User.Identity?.Name;
    public string? RoleName => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsInRole(string roleName)
    {
        return _httpContextAccessor.HttpContext?.User.IsInRole(roleName) == true;
    }
}
