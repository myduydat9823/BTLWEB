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
        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Clamp(filter.PageSize, 1, 50);

        var query = BuildAdminPostQuery();

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
            .OrderByDescending(x => x.CreatedAtUtc)
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

    private IQueryable<Post> BuildAdminPostQuery()
    {
        return _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Where(x => !x.IsDeleted);
    }
}
