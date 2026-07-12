using BTLWEB.Repositories.Interfaces;
using BTLWEB.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BTLWEB.Controllers;

public class CategoryController : Controller
{
    private readonly IPostRepository _postRepository;

    public CategoryController(IPostRepository postRepository)
    {
        _postRepository = postRepository;
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

        return View(model);
    }
}
