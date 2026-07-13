using BTLWEB.ViewModels.Competition;

namespace BTLWEB.ViewModels;

public class CategoryIndexViewModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public List<PostCardViewModel> Posts { get; set; } = [];
    public List<CompetitionListViewModel> Competitions { get; set; } = [];
}