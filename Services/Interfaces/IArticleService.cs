using BTLWEB.ViewModels;

namespace BTLWEB.Services.Interfaces;

public interface IArticleService
{
    Task<ArticleListViewModel> GetAdminListAsync(ArticleFilterViewModel filter);
    Task<ArticleListViewModel> GetDeletedListAsync(ArticleFilterViewModel filter);
    Task<ArticleCreateViewModel> BuildCreateViewModelAsync(ArticleCreateViewModel? model = null);
    Task<ArticleEditViewModel?> BuildEditViewModelAsync(int id);
    Task<ArticleEditViewModel> BuildEditViewModelAsync(ArticleEditViewModel model);
    Task<ArticlePreviewViewModel?> BuildPreviewViewModelAsync(int id, bool includeDeleted = false);
    Task<OperationResult<int>> CreateAsync(ArticleCreateViewModel model, int authorId, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateAsync(ArticleEditViewModel model, int actorUserId, string? ipAddress = null, string? userAgent = null, CancellationToken cancellationToken = default);
    Task<OperationResult> ChangeStatusAsync(int id, string status, int actorUserId, string? ipAddress = null, string? userAgent = null);
    Task<OperationResult> SoftDeleteAsync(int id, int deletedByUserId, string? ipAddress = null, string? userAgent = null);
    Task<OperationResult> RestoreAsync(int id, int actorUserId, string? ipAddress = null, string? userAgent = null);
}
