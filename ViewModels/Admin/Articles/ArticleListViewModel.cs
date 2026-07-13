namespace BTLWEB.ViewModels;

public class ArticleListViewModel
{
    public ArticleFilterViewModel Filter { get; set; } = new();
    public PagedResult<ArticleListItemViewModel> Articles { get; set; } = new();
    public IReadOnlyList<ArticleCategoryOptionViewModel> Categories { get; set; } = [];
    public IReadOnlyList<ArticleStatusOptionViewModel> Statuses { get; set; } = [];
}
