using BTLWEB.ViewModels;

namespace BTLWEB.Services.Interfaces;

public interface IArticleService
{
    Task<ArticleCreateViewModel> BuildCreateViewModelAsync(ArticleCreateViewModel? model = null);
    Task<OperationResult<int>> CreateAsync(ArticleCreateViewModel model, int authorId, CancellationToken cancellationToken = default);
}
