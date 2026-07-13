using BTLWEB.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BTLWEB.Controllers.Posts;

public class PostController : Controller
{
    private readonly IPostRepository _postRepository;

    public PostController(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    [HttpGet("Post/Detail/{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return NotFound();
        }

        var post = await _postRepository.GetDetailBySlugAsync(slug);
        if (post is null)
        {
            return NotFound();
        }

        await _postRepository.IncrementViewCountAsync(post.Id);
        post.ViewCount++;

        return View("~/Views/Post/Detail.cshtml", post);
    }
}
