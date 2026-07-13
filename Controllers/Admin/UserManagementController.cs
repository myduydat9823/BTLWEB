using BTLWEB.Common;
using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTLWEB.Controllers.Admin;

[Authorize(Roles = RoleNames.Admin)]
[Route("Admin/UserManagement")]
public class UserManagementController : Controller
{
    private readonly IUserAccountService _userAccountService;
    private readonly ICurrentUserService _currentUserService;

    public UserManagementController(IUserAccountService userAccountService, ICurrentUserService currentUserService)
    {
        _userAccountService = userAccountService;
        _currentUserService = currentUserService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, string? role, string? status, int page = 1)
    {
        var model = await _userAccountService.GetUserListAsync(search, role, status, page);
        if (IsAjaxRequest())
        {
            return PartialView("~/Views/Admin/UserManagement/_UserListTable.cshtml", model);
        }

        return View("~/Views/Admin/UserManagement/Index.cshtml", model);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var result = await _userAccountService.GetUserDetailsAsync(id);
        if (!result.Succeeded || result.Data is null)
        {
            return NotFound();
        }

        return View("~/Views/Admin/UserManagement/Details.cshtml", result.Data);
    }

    [HttpGet("{id:int}/ChangeRole")]
    public async Task<IActionResult> ChangeRole(int id)
    {
        var result = await _userAccountService.GetChangeRoleAsync(id);
        if (!result.Succeeded || result.Data is null)
        {
            return NotFound();
        }

        return View("~/Views/Admin/UserManagement/ChangeRole.cshtml", result.Data);
    }

    [HttpPost("{id:int}/ChangeRole")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(int id, ChangeUserRoleViewModel model)
    {
        if (id != model.UserId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var reload = await _userAccountService.GetChangeRoleAsync(id);
            model.Roles = reload.Data?.Roles ?? [];
            return View("~/Views/Admin/UserManagement/ChangeRole.cshtml", model);
        }

        var result = await _userAccountService.ChangeRoleAsync(model, _currentUserService.UserId);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            var reload = await _userAccountService.GetChangeRoleAsync(id);
            model.Roles = reload.Data?.Roles ?? [];
            return View("~/Views/Admin/UserManagement/ChangeRole.cshtml", model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/Lock")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Lock(int id)
    {
        return SetActiveAndRedirectAsync(id, false);
    }

    [HttpPost("{id:int}/Unlock")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Unlock(int id)
    {
        return SetActiveAndRedirectAsync(id, true);
    }

    [HttpPost("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userAccountService.SoftDeleteAsync(id, _currentUserService.UserId);
        SetTempMessage(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var result = await _userAccountService.RestoreAsync(id);
        SetTempMessage(result);
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<IActionResult> SetActiveAndRedirectAsync(int id, bool isActive)
    {
        var result = await _userAccountService.SetActiveAsync(id, isActive, _currentUserService.UserId);
        SetTempMessage(result);
        return RedirectToAction(nameof(Details), new { id });
    }

    private void SetTempMessage(OperationResult result)
    {
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }
    }

    private bool IsAjaxRequest()
    {
        return string.Equals(Request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
    }
}
