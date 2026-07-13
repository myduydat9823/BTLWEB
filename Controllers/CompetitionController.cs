using BTLWEB.Models.Competition;
using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;
using BTLWEB.ViewModels.Entry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BTLWEB.Controllers;

public class CompetitionController : Controller
{
  private readonly ICompetitionService _competitionService;
  private readonly IEntryService _entryService;
  private readonly ICurrentUserService _currentUserService;

  public CompetitionController(
      ICompetitionService competitionService,
      IEntryService entryService,
      ICurrentUserService currentUserService)
  {
    _competitionService = competitionService;
    _entryService = entryService;
    _currentUserService = currentUserService;
  }

  [HttpGet("cuoc-thi")]
  public async Task<IActionResult> Index()
  {
    var competitions = await _competitionService.GetAllAsync();
    return View("~/Views/Competition/Index.cshtml", competitions);
  }

  [HttpGet("cuoc-thi/{id:int}")]
  public async Task<IActionResult> Details(int id)
  {
    var competition = await _competitionService.GetByIdAsync(id);
    if (competition is null)
    {
      return NotFound();
    }

    return View("~/Views/Competition/Details.cshtml", competition);
  }

  [Authorize]
  [HttpGet("cuoc-thi/{id:int}/submit")]
  public async Task<IActionResult> Submit(int id)
  {
    var competition = await _competitionService.GetByIdAsync(id);
    if (competition is null)
    {
      return NotFound();
    }

    if (competition.Status != CompetitionStatus.OpenForSubmission)
    {
      TempData["ErrorMessage"] = "Cuộc thi không trong trạng thái mở nhận bài.";
      return RedirectToAction(nameof(Details), new { id });
    }

    var model = new SubmitEntryViewModel
    {
      CompetitionId = id
    };

    return View("~/Views/Competition/Submit.cshtml", model);
  }

  [Authorize]
  [HttpPost("cuoc-thi/{id:int}/submit")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Submit(int id, SubmitEntryViewModel model)
  {
    if (id != model.CompetitionId)
    {
      return BadRequest();
    }

    if (!ModelState.IsValid)
    {
      return View("~/Views/Competition/Submit.cshtml", model);
    }

    var result = await _entryService.SubmitAsync(model, _currentUserService.UserId!.Value);
    if (!result.Succeeded)
    {
      ModelState.AddModelError(string.Empty, result.Message);
      return View("~/Views/Competition/Submit.cshtml", model);
    }

    TempData["SuccessMessage"] = result.Message;
    return RedirectToAction(nameof(Details), new { id });
  }

  [Authorize]
  [HttpGet("bai-du-thi-cua-toi")]
  public async Task<IActionResult> MyEntries()
  {
    var entries = await _entryService.GetMyEntriesAsync(_currentUserService.UserId!.Value);
    return View("~/Views/Competition/MyEntries.cshtml", entries);
  }
}