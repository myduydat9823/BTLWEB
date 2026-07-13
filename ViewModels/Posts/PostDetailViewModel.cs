namespace BTLWEB.ViewModels;

public class PostDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public string? AuthorName { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public List<PostCardViewModel> RelatedPosts { get; set; } = [];
}
