using BTLWEB.Common;
using BTLWEB.Models.Competition;
using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;
using BTLWEB.ViewModels.Competition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTLWEB.Controllers.Admin;

[Authorize(Roles = RoleNames.Admin)]
[Route("Admin/Competition")]
public class CompetitionController : Controller
{
  private readonly ICompetitionService _competitionService;
  private readonly ICurrentUserService _currentUserService;

  public CompetitionController(
      ICompetitionService competitionService,
      ICurrentUserService currentUserService)
  {
    _competitionService = competitionService;
    _currentUserService = currentUserService;
  }

  [HttpGet("")]
  public async Task<IActionResult> Index()
  {
    var competitions = await _competitionService.GetAllAsync();
    return View("~/Views/Admin/Competition/Index.cshtml", competitions);
  }

  [HttpGet("Create")]
  public IActionResult Create()
  {
    return View("~/Views/Admin/Competition/Create.cshtml", new CreateCompetitionViewModel());
  }

  [HttpPost("Create")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(CreateCompetitionViewModel model)
  {
    if (!ModelState.IsValid)
    {
      return View("~/Views/Admin/Competition/Create.cshtml", model);
    }

    var result = await _competitionService.CreateAsync(model, _currentUserService.UserId!.Value);
    if (!result.Succeeded)
    {
      ModelState.AddModelError(string.Empty, result.Message);
      return View("~/Views/Admin/Competition/Create.cshtml", model);
    }

    TempData["SuccessMessage"] = result.Message;
    return Redirect("/cuoc-thi");
  }

  [HttpGet("{id:int}/Edit")]
  public async Task<IActionResult> Edit(int id)
  {
    var model = await _competitionService.GetForEditAsync(id);
    if (model is null)
    {
      return NotFound();
    }

    return View("~/Views/Admin/Competition/Edit.cshtml", model);
  }

  [HttpPost("{id:int}/Edit")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(int id, CreateCompetitionViewModel model)
  {
    if (id != model.Id)
    {
      return BadRequest();
    }

    if (!ModelState.IsValid)
    {
      return View("~/Views/Admin/Competition/Edit.cshtml", model);
    }

    var result = await _competitionService.UpdateAsync(model);
    if (!result.Succeeded)
    {
      ModelState.AddModelError(string.Empty, result.Message);
      return View("~/Views/Admin/Competition/Edit.cshtml", model);
    }

    TempData["SuccessMessage"] = result.Message;
    return RedirectToAction(nameof(Index));
  }

  [HttpPost("{id:int}/Delete")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Delete(int id)
  {
    var result = await _competitionService.DeleteAsync(id);
    SetTempMessage(result);
    return RedirectToAction(nameof(Index));
  }

  [HttpPost("{id:int}/ChangeStatus")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> ChangeStatus(int id, int newStatus)
  {
    var result = await _competitionService.ChangeStatusAsync(id, newStatus);
    SetTempMessage(result);
    return RedirectToAction(nameof(Index));
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
}