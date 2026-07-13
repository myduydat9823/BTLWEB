using BTLWEB.Data;
using BTLWEB.Models;
using BTLWEB.Repositories.Interfaces;
using BTLWEB.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Repositories;

public class AdminPostRepository : IAdminPostRepository
{
    private readonly AppDbContext _dbContext;

    public AdminPostRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Post?> GetByIdAsync(int id)
    {
        return BuildAdminPostQuery()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<Post?> GetDeletedByIdAsync(int id)
    {
        return BuildAdminPostQuery(includeDeleted: true)
            .Where(x => x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<Post?> GetBySlugAsync(string slug)
    {
        var now = DateTime.UtcNow;

        return _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Where(x => !x.IsDeleted)
            .Where(x => x.Status == PostStatus.Published)
            .Where(x => x.PublishedAt != null && x.PublishedAt <= now)
            .FirstOrDefaultAsync(x => x.Slug == slug);
    }

    public Task<bool> SlugExistsAsync(string slug, int? excludedPostId = null)
    {
        var query = _dbContext.Posts.AsNoTracking().Where(x => x.Slug == slug);
        if (excludedPostId is not null)
        {
            query = query.Where(x => x.Id != excludedPostId.Value);
        }

        return query.AnyAsync();
    }

    public async Task AddAsync(Post post)
    {
        await _dbContext.Posts.AddAsync(post);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Post post)
    {
        _dbContext.Posts.Update(post);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<PagedResult<Post>> GetAdminArticlesAsync(ArticleFilterViewModel filter)
    {
        return await GetArticlesAsync(filter, includeDeleted: false);
    }

    public async Task<PagedResult<Post>> GetDeletedArticlesAsync(ArticleFilterViewModel filter)
    {
        return await GetArticlesAsync(filter, includeDeleted: true);
    }

    public Task<List<Post>> GetPublishedArticlesAsync(int take, int? categoryId = null)
    {
        var now = DateTime.UtcNow;
        var query = _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Where(x => !x.IsDeleted)
            .Where(x => x.Status == PostStatus.Published)
            .Where(x => x.PublishedAt != null && x.PublishedAt <= now);

        if (categoryId is > 0)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        return query
            .OrderByDescending(x => x.PublishedAt)
            .Take(Math.Max(take, 0))
            .ToListAsync();
    }

    public Task<bool> CategoryExistsAsync(int categoryId)
    {
        return _dbContext.Categories.AsNoTracking().AnyAsync(x => x.Id == categoryId);
    }

    public Task<List<Post>> GetPostsByAuthorIdAsync(int authorId)
    {
        return _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.AuthorId == authorId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public Task<List<ArticleCategoryOptionViewModel>> GetCategoryOptionsAsync()
    {
        return _dbContext.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ArticleCategoryOptionViewModel
            {
                CategoryId = x.Id,
                Name = x.Name
            })
            .ToListAsync();
    }

    public async Task AddLogAsync(ArticleAdminLog log)
    {
        await _dbContext.ArticleAdminLogs.AddAsync(log);
        await _dbContext.SaveChangesAsync();
    }

    public Task<List<ArticleAdminLog>> GetLogsByPostIdAsync(int postId, int take = 30)
    {
        return _dbContext.ArticleAdminLogs
            .AsNoTracking()
            .Include(x => x.ActorUser)
            .Where(x => x.PostId == postId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync();
    }

    private async Task<PagedResult<Post>> GetArticlesAsync(ArticleFilterViewModel filter, bool includeDeleted)
    {
        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Clamp(filter.PageSize, 1, 50);

        var query = BuildAdminPostQuery(includeDeleted)
            .Where(x => x.IsDeleted == includeDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var keyword = filter.Search.Trim();
            query = query.Where(x => x.Title.Contains(keyword));
        }

        if (filter.CategoryId is > 0)
        {
            query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(x => x.Status == filter.Status);
        }

        if (filter.IsFeatured is not null)
        {
            query = query.Where(x => x.IsFeatured == filter.IsFeatured.Value);
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => includeDeleted ? x.DeletedAtUtc : x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Post>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    private IQueryable<Post> BuildAdminPostQuery(bool includeDeleted = false)
    {
        var query = _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.DeletedByUser);

        return includeDeleted ? query : query.Where(x => !x.IsDeleted);
    }
}
