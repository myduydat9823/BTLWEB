using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTLWEB.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly IUserAccountService _userAccountService;
    private readonly ICurrentUserService _currentUserService;

    public AccountController(IUserAccountService userAccountService, ICurrentUserService currentUserService)
    {
        _userAccountService = userAccountService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Challenge();
        }

        var result = await _userAccountService.GetProfileAsync(userId.Value);
        if (!result.Succeeded || result.Data is null)
        {
            return NotFound();
        }

        return View(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Challenge();
        }

        var result = await _userAccountService.GetEditProfileAsync(userId.Value);
        if (!result.Succeeded || result.Data is null)
        {
            return NotFound();
        }

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _userAccountService.UpdateProfileAsync(userId.Value, model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _userAccountService.ChangePasswordAsync(userId.Value, model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Profile));
    }
}
