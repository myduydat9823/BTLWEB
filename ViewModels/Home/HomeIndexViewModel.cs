namespace BTLWEB.ViewModels;

public class HomeIndexViewModel
{
    public PostCardViewModel? MainFeaturedPost { get; set; }
    public List<PostCardViewModel> FeaturedPosts { get; set; } = [];
    public List<PostCardViewModel> LatestPosts { get; set; } = [];
    public List<PostCardViewModel> MostViewedPosts { get; set; } = [];
    public List<PostCardViewModel> NewsPosts { get; set; } = [];
    public List<PostCardViewModel> HighlightPhotoPosts { get; set; } = [];
    public List<PostCardViewModel> ContestPosts { get; set; } = [];
    public List<PostCardViewModel> ExhibitionPosts { get; set; } = [];
    public List<PostCardViewModel> LifePhotoPosts { get; set; } = [];
    public List<PostCardViewModel> TravelCulturePosts { get; set; } = [];
    public List<PostCardViewModel> VapaPosts { get; set; } = [];
    public List<PostCardViewModel> MediaPosts { get; set; } = [];
}
