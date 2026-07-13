using System.ComponentModel.DataAnnotations;

namespace BTLWEB.ViewModels;

public class ArticleEditViewModel : ArticleCreateViewModel
{
    [Required]
    public int Id { get; set; }

    public string Slug { get; set; } = string.Empty;
    public string? ExistingThumbnailUrl { get; set; }
}
