using BTLWEB.Data;
using BTLWEB.Models;
using BTLWEB.Repositories.Interfaces;
using BTLWEB.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Repositories;

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<PostRepository> _logger;

    public PostRepository(AppDbContext dbContext, ILogger<PostRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public Task<List<PostCardViewModel>> GetFeaturedPostsAsync(int take)
    {
        return ExecuteListAsync(() =>
            BuildPublishedPostsQuery()
                .Where(x => x.IsFeatured)
                .OrderByDescending(x => x.PublishedAt)
                .Skip(1)
                .Take(take)
                .Select(MapToCard())
                .ToListAsync());
    }

    public Task<List<PostCardViewModel>> GetLatestPostsAsync(int take)
    {
        return ExecuteListAsync(() =>
            BuildPublishedPostsQuery()
                .OrderByDescending(x => x.PublishedAt)
                .Take(take)
                .Select(MapToCard())
                .ToListAsync());
    }

    public Task<List<PostCardViewModel>> GetMostViewedPostsAsync(int take)
    {
        return ExecuteListAsync(() =>
            BuildPublishedPostsQuery()
                .OrderByDescending(x => x.ViewCount)
                .ThenByDescending(x => x.PublishedAt)
                .Take(take)
                .Select(MapToCard())
                .ToListAsync());
    }

    public Task<List<PostCardViewModel>> GetPostsByCategorySlugAsync(string categorySlug, int take)
    {
        return ExecuteListAsync(() =>
            BuildPublishedPostsQuery()
                .Where(x => x.Category != null && x.Category.Slug == categorySlug)
                .OrderByDescending(x => x.PublishedAt)
                .Take(take)
                .Select(MapToCard())
                .ToListAsync());
    }

    public Task<PostCardViewModel?> GetMainFeaturedPostAsync()
    {
        return ExecuteSingleAsync(() =>
            BuildPublishedPostsQuery()
                .Where(x => x.IsFeatured)
                .OrderByDescending(x => x.PublishedAt)
                .Select(MapToCard())
                .FirstOrDefaultAsync());
    }

    private IQueryable<Post> BuildPublishedPostsQuery()
    {
        return _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.Status == PostStatus.Published);
    }

    private static System.Linq.Expressions.Expression<Func<Post, PostCardViewModel>> MapToCard()
    {
        return x => new PostCardViewModel
        {
            Title = x.Title,
            Slug = x.Slug,
            Summary = x.Summary,
            ThumbnailUrl = x.ThumbnailUrl,
            CategoryName = x.Category != null ? x.Category.Name : string.Empty,
            CategorySlug = x.Category != null ? x.Category.Slug : string.Empty,
            PublishedAt = x.PublishedAt,
            ViewCount = x.ViewCount,
            IsFeatured = x.IsFeatured,
            Status = x.Status
        };
    }

    private async Task<List<PostCardViewModel>> ExecuteListAsync(Func<Task<List<PostCardViewModel>>> query)
    {
        try
        {
            return await query();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Khong the tai danh sach bai viet tu database.");
            return [];
        }
    }

    private async Task<PostCardViewModel?> ExecuteSingleAsync(Func<Task<PostCardViewModel?>> query)
    {
        try
        {
            return await query();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Khong the tai bai viet noi bat tu database.");
            return null;
        }
    }
}
