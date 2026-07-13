using BTLWEB.ViewModels;

namespace BTLWEB.Services.Interfaces;

public interface IArticleService
{
    Task<ArticleListViewModel> GetAdminListAsync(ArticleFilterViewModel filter);
    Task<ArticleCreateViewModel> BuildCreateViewModelAsync(ArticleCreateViewModel? model = null);
    Task<ArticleEditViewModel?> BuildEditViewModelAsync(int id);
    Task<ArticleEditViewModel> BuildEditViewModelAsync(ArticleEditViewModel model);
    Task<OperationResult<int>> CreateAsync(ArticleCreateViewModel model, int authorId, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateAsync(ArticleEditViewModel model, CancellationToken cancellationToken = default);
}
