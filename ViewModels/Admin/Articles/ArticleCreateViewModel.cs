using System.ComponentModel.DataAnnotations;
using BTLWEB.Models;
using BTLWEB.ViewModels.Validation;

namespace BTLWEB.ViewModels;

public class ArticleCreateViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
    [StringLength(250, ErrorMessage = "Tiêu đề tối đa 250 ký tự.")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mô tả ngắn.")]
    [StringLength(500, ErrorMessage = "Mô tả ngắn tối đa 500 ký tự.")]
    [Display(Name = "Mô tả ngắn")]
    public string Summary { get; set; } = string.Empty;

    [RequiredPlainText(ErrorMessage = "Vui lòng nhập nội dung bài viết.")]
    [Display(Name = "Nội dung")]
    public string Content { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }

    [Display(Name = "Ảnh đại diện")]
    public IFormFile? Thumbnail { get; set; }

    [Display(Name = "Bài nổi bật")]
    public bool IsFeatured { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
    [Display(Name = "Trạng thái")]
    public string Status { get; set; } = PostStatus.Draft;

    [DataType(DataType.DateTime)]
    [Display(Name = "Thời gian xuất bản")]
    public DateTime? PublishedAt { get; set; }

    [StringLength(250, ErrorMessage = "Meta title tối đa 250 ký tự.")]
    [Display(Name = "Meta title")]
    public string? MetaTitle { get; set; }

    [StringLength(500, ErrorMessage = "Meta description tối đa 500 ký tự.")]
    [Display(Name = "Meta description")]
    public string? MetaDescription { get; set; }

    public IReadOnlyList<ArticleCategoryOptionViewModel> Categories { get; set; } = [];
    public IReadOnlyList<ArticleStatusOptionViewModel> Statuses { get; set; } = [];

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        Title = Title.Trim();
        Summary = Summary.Trim();
        MetaTitle = string.IsNullOrWhiteSpace(MetaTitle) ? null : MetaTitle.Trim();
        MetaDescription = string.IsNullOrWhiteSpace(MetaDescription) ? null : MetaDescription.Trim();

        if (!PostStatus.IsReviewStatus(Status) && !PostStatus.All.Contains(Status))
        {
            yield return new ValidationResult("Trạng thái bài viết không hợp lệ.", [nameof(Status)]);
        }

        if (IsFeatured && !PostStatus.IsVisibleStatus(Status))
        {
            yield return new ValidationResult("Chỉ bài đã được duyệt mới được đánh dấu nổi bật.", [nameof(IsFeatured)]);
        }
    }
}
