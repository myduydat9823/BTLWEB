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

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] ArticleFilterViewModel filter)
    {
        var model = await _articleService.GetAdminListAsync(filter);
        return View("~/Views/Admin/Articles/Index.cshtml", model);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var model = await _articleService.BuildCreateViewModelAsync();
        return View("~/Views/Admin/Articles/Create.cshtml", model);
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _articleService.BuildEditViewModelAsync(id);
        if (model is null)
        {
            TempData["ErrorMessage"] = "Bài viết không tồn tại hoặc đã bị xóa.";
            return RedirectToAction(nameof(Index));
        }

        return View("~/Views/Admin/Articles/Edit.cshtml", model);
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
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ArticleEditViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = await _articleService.BuildEditViewModelAsync(model);
            return View("~/Views/Admin/Articles/Edit.cshtml", invalidModel);
        }

        var result = await _articleService.UpdateAsync(model, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            var invalidModel = await _articleService.BuildEditViewModelAsync(model);
            return View("~/Views/Admin/Articles/Edit.cshtml", invalidModel);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/ChangeStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, string status)
    {
        var result = await _articleService.ChangeStatusAsync(id, status);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Challenge();
        }

        var result = await _articleService.SoftDeleteAsync(id, userId.Value);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
