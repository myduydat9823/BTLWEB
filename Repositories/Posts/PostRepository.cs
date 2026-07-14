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

    public Task<List<PostCardViewModel>> GetPostsByCategoryIdAsync(int categoryId)
    {
        return ExecuteListAsync(() =>
            BuildPublishedPostsQuery()
                .Where(x => x.CategoryId == categoryId)
                .OrderByDescending(x => x.PublishedAt)
                .Select(MapToCard())
                .ToListAsync());
    }

    public async Task<List<PostCardViewModel>> GetPostsByCategorySlugAsync(string categorySlug, int take)
    {
        var category = await GetCategoryBySlugAsync(categorySlug);
        if (category is null)
        {
            return [];
        }

        return await ExecuteListAsync(() =>
            BuildPublishedPostsQuery()
                .Where(x => x.CategoryId == category.Id)
                .OrderByDescending(x => x.PublishedAt)
                .Take(take)
                .Select(MapToCard())
                .ToListAsync());
    }

    public async Task<CategorySummaryViewModel?> GetCategoryBySlugAsync(string categorySlug)
    {
        try
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Where(x => x.Slug == categorySlug)
                .Select(x => new CategorySummaryViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug
                })
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Khong the tai chuyen muc tu database.");
            return null;
        }
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

    public async Task<PostDetailViewModel?> GetDetailBySlugAsync(string slug)
    {
        try
        {
            var post = await BuildPublishedPostsQuery()
                .Include(x => x.Author)
                .Where(x => x.Slug == slug)
                .Select(x => new PostDetailViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Slug = x.Slug,
                    Summary = x.Summary,
                    Content = x.Content ?? string.Empty,
                    ThumbnailUrl = x.ThumbnailUrl,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category != null ? x.Category.Name : string.Empty,
                    CategorySlug = x.Category != null ? x.Category.Slug : string.Empty,
                    AuthorName = x.Author != null ? x.Author.FullName : null,
                    PublishedAt = x.PublishedAt,
                    ViewCount = x.ViewCount,
                    MetaTitle = x.MetaTitle,
                    MetaDescription = x.MetaDescription
                })
                .FirstOrDefaultAsync();

            if (post is null)
            {
                return null;
            }

            post.RelatedPosts = await BuildPublishedPostsQuery()
                .Where(x => x.CategoryId == post.CategoryId && x.Id != post.Id)
                .OrderByDescending(x => x.PublishedAt)
                .Take(4)
                .Select(MapToCard())
                .ToListAsync();

            return post;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Khong the tai chi tiet bai viet {Slug}.", slug);
            return null;
        }
    }

    public async Task IncrementViewCountAsync(int id)
    {
        try
        {
            await _dbContext.Posts
                .Where(x => x.Id == id && !x.IsDeleted)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + 1));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Khong the tang luot xem bai viet {PostId}.", id);
        }
    }

    private IQueryable<Post> BuildPublishedPostsQuery()
    {
        var now = DateTime.UtcNow;

        return _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => PostStatus.VisibleStatuses.Contains(x.Status)
                && !x.IsDeleted
                && x.PublishedAt != null
                && x.PublishedAt <= now);
    }

    private static System.Linq.Expressions.Expression<Func<Post, PostCardViewModel>> MapToCard()
    {
        return x => new PostCardViewModel
        {
            Title = x.Title,
            Slug = x.Slug,
            Summary = x.Summary,
            ThumbnailUrl = x.ThumbnailUrl,
            CategoryId = x.CategoryId,
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
