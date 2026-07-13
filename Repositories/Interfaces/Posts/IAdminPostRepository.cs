using BTLWEB.Models;
using BTLWEB.ViewModels;

namespace BTLWEB.Repositories.Interfaces;

public interface IAdminPostRepository
{
    Task<Post?> GetByIdAsync(int id);
    Task<Post?> GetBySlugAsync(string slug);
    Task<bool> SlugExistsAsync(string slug, int? excludedPostId = null);
    Task AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task<PagedResult<Post>> GetAdminArticlesAsync(ArticleFilterViewModel filter);
    Task<List<Post>> GetPublishedArticlesAsync(int take, int? categoryId = null);
    Task<bool> CategoryExistsAsync(int categoryId);
    Task<List<ArticleCategoryOptionViewModel>> GetCategoryOptionsAsync();
}
