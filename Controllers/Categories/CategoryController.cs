using BTLWEB.Repositories.Interfaces;
using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BTLWEB.Controllers;

public class CategoryController : Controller
{
    private readonly IPostRepository _postRepository;
    private readonly ICompetitionService _competitionService;

    public CategoryController(
        IPostRepository postRepository,
        ICompetitionService competitionService)
    {
        _postRepository = postRepository;
        _competitionService = competitionService;
    }

    [HttpGet("Category/{slug}")]
    public async Task<IActionResult> Index(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return NotFound();
        }

        var category = await _postRepository.GetCategoryBySlugAsync(slug);
        if (category is null)
        {
            return NotFound();
        }

        var model = new CategoryIndexViewModel
        {
            CategoryId = category.Id,
            CategoryName = category.Name,
            CategorySlug = category.Slug,
            Posts = await _postRepository.GetPostsByCategoryIdAsync(category.Id)
        };

        if (slug == "cuoc-thi-anh")
        {
            model.Competitions = await _competitionService.GetAllAsync();
        }

        return View(model);
    }
}
