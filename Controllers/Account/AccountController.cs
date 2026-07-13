using BTLWEB.Models;
using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;
using BTLWEB.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTLWEB.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly IUserAccountService _userAccountService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IArticleService _articleService;

    public AccountController(IUserAccountService userAccountService, ICurrentUserService currentUserService, IArticleService articleService)
    {
        _userAccountService = userAccountService;
        _currentUserService = currentUserService;
        _articleService = articleService;
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
    public async Task<IActionResult> Create()
    {
        var accountModel = new AccountCreateArticleViewModel();
        var adminModel = await _articleService.BuildCreateViewModelAsync();

        accountModel.Categories = adminModel.Categories;

        var target = accountModel.Categories?
            .FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Name) && c.Name.Replace(" ", "").ToLowerInvariant().Contains("anh") && c.Name.Replace(" ", "").ToLowerInvariant().Contains("doi"));
        if (target is not null)
        {
            accountModel.CategoryId = target.CategoryId;
            accountModel.CategoryName = target.Name;
        }
        else
        {
            accountModel.CategoryName = "Ảnh và Đời sống";
        }

        return View(accountModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AccountCreateArticleViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var authorId = _currentUserService.UserId;
        if (authorId is null)
        {
            return Challenge();
        }

        var articleModel = new ArticleCreateViewModel
        {
            Title = model.Title,
            Summary = model.Summary,
            Content = model.Content,
            CategoryId = model.CategoryId,
            Thumbnail = model.Thumbnail,
            Status = PostStatus.Pending,
            MetaTitle = model.MetaTitle,
            MetaDescription = model.MetaDescription,
            PublishedAt = null,
            IsFeatured = false
        };

        var result = await _articleService.CreateAsync(articleModel, authorId.Value, GetIpAddress(), GetUserAgent(), cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = "Đăng bài thành công. Bài viết sẽ được gửi tới quản trị viên để duyệt.";
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

    private string? GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        return Request.Headers.UserAgent.ToString();
    }
}
