using System.Diagnostics;
using BTLWEB.Models;
using BTLWEB.Repositories.Interfaces;
using BTLWEB.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BTLWEB.Controllers;

public class HomeController : Controller
{
    private readonly IPostRepository _postRepository;

    public HomeController(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeIndexViewModel
        {
            MainFeaturedPost = await _postRepository.GetMainFeaturedPostAsync(),
            FeaturedPosts = await _postRepository.GetFeaturedPostsAsync(4),
            LatestPosts = await _postRepository.GetLatestPostsAsync(6),
            MostViewedPosts = await _postRepository.GetMostViewedPostsAsync(6),
            NewsPosts = await _postRepository.GetPostsByCategorySlugAsync("tin-tuc", 6),
            HighlightPhotoPosts = await _postRepository.GetPostsByCategorySlugAsync("anh-noi-bat", 6),
            ContestPosts = await _postRepository.GetPostsByCategorySlugAsync("cuoc-thi-anh", 6),
            ExhibitionPosts = await _postRepository.GetPostsByCategorySlugAsync("trien-lam-anh-online", 6),
            LifePhotoPosts = await _postRepository.GetPostsByCategorySlugAsync("anh-va-doi-song", 6),
            TravelCulturePosts = await _postRepository.GetPostsByCategorySlugAsync("du-lich-van-hoa-xa-hoi", 6),
            VapaPosts = await _postRepository.GetPostsByCategorySlugAsync("vapa", 6),
            MediaPosts = await _postRepository.GetPostsByCategorySlugAsync("media", 6)
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
