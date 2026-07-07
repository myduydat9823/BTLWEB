using BTLWEB.ViewModels;

namespace BTLWEB.Repositories.Interfaces;

public interface IPostRepository
{
    Task<List<PostCardViewModel>> GetFeaturedPostsAsync(int take);
    Task<List<PostCardViewModel>> GetLatestPostsAsync(int take);
    Task<List<PostCardViewModel>> GetMostViewedPostsAsync(int take);
    Task<List<PostCardViewModel>> GetPostsByCategorySlugAsync(string categorySlug, int take);
    Task<PostCardViewModel?> GetMainFeaturedPostAsync();
}
