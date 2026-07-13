using BTLWEB.Common;
using BTLWEB.Models;
using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTLWEB.Controllers.Admin;

[Authorize(Roles = RoleNames.Admin)]
[Route("Admin/Articles")]
public class ArticlesController : Controller
{
    private readonly IArticleService _articleService;
    private readonly ICurrentUserService _currentUserService;

    public ArticlesController(IArticleService articleService, ICurrentUserService currentUserService)
    {
        _articleService = articleService;
        _currentUserService = currentUserService;
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var model = await _articleService.BuildCreateViewModelAsync();
        return View("~/Views/Admin/Articles/Create.cshtml", model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ArticleCreateViewModel model, string command, CancellationToken cancellationToken)
    {
        model.Status = command == "publish" ? PostStatus.Published : PostStatus.Draft;

        ModelState.Clear();
        TryValidateModel(model);

        if (!ModelState.IsValid)
        {
            var invalidModel = await _articleService.BuildCreateViewModelAsync(model);
            return View("~/Views/Admin/Articles/Create.cshtml", invalidModel);
        }

        var authorId = _currentUserService.UserId;
        if (authorId is null)
        {
            return Challenge();
        }

        var result = await _articleService.CreateAsync(model, authorId.Value, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            var invalidModel = await _articleService.BuildCreateViewModelAsync(model);
            return View("~/Views/Admin/Articles/Create.cshtml", invalidModel);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Create));
    }
}
