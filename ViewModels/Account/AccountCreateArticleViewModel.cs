using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using BTLWEB.Models;
using BTLWEB.ViewModels;
using BTLWEB.ViewModels.Validation;

namespace BTLWEB.ViewModels.Account;

public class AccountCreateArticleViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
    [StringLength(250, ErrorMessage = "Tiêu đề tối đa 250 ký tự.")]
    [Display(Name = "Tên tác phẩm")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn ảnh.")]
    [Display(Name = "Ảnh bài viết")]
    public IFormFile? Thumbnail { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mô tả ngắn.")]
    [StringLength(500, ErrorMessage = "Mô tả ngắn tối đa 500 ký tự.")]
    [Display(Name = "Mô tả ngắn")]
    public string? Summary { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
    [StringLength(20000, ErrorMessage = "Nội dung tối đa 20000 ký tự.")]
    [Display(Name = "Nội dung")]
    public string? Content { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Danh mục không hợp lệ.")]
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = "Ảnh và Đời sống";

    public IReadOnlyList<ArticleCategoryOptionViewModel> Categories { get; set; } = [];

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
{
    // Chuẩn hóa dữ liệu
    Title = Title?.Trim() ?? "";
    Summary = Summary?.Trim();
    Content = Content?.Trim();
    MetaTitle = MetaTitle?.Trim();
    MetaDescription = MetaDescription?.Trim();

    // Chống XSS
    var xssPattern = @"<[^>]+>";

    if (Regex.IsMatch(Title, xssPattern))
    {
        yield return new ValidationResult(
            "Tiêu đề không được chứa mã HTML.",
            new[] { nameof(Title) });
    }

    if (!string.IsNullOrWhiteSpace(Summary) &&
        Regex.IsMatch(Summary, xssPattern))
    {
        yield return new ValidationResult(
            "Mô tả ngắn không được chứa mã HTML.",
            new[] { nameof(Summary) });
    }

    if (!string.IsNullOrWhiteSpace(Content) &&
        Regex.IsMatch(Content, xssPattern))
    {
        yield return new ValidationResult(
            "Nội dung không được chứa mã HTML.",
            new[] { nameof(Content) });
    }

    // Kiểm tra ảnh
    if (Thumbnail is null)
    {
        yield return new ValidationResult(
            "Vui lòng chọn file ảnh.",
            new[] { nameof(Thumbnail) });

        yield break;
    }

    // Kiểm tra phần mở rộng
    var extension = Path.GetExtension(Thumbnail.FileName).ToLowerInvariant();

    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

    if (!allowedExtensions.Contains(extension))
    {
        yield return new ValidationResult(
            "Chỉ chấp nhận file JPG, JPEG, PNG hoặc WEBP.",
            new[] { nameof(Thumbnail) });
    }

    // Kiểm tra MIME Type
    var allowedTypes = new[]
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    if (string.IsNullOrWhiteSpace(Thumbnail.ContentType) ||
        !allowedTypes.Contains(Thumbnail.ContentType.ToLowerInvariant()))
    {
        yield return new ValidationResult(
            "Định dạng ảnh không hợp lệ.",
            new[] { nameof(Thumbnail) });
    }

    // Kiểm tra dung lượng
    const long maxBytes = 5 * 1024 * 1024;

    if (Thumbnail.Length > maxBytes)
    {
        yield return new ValidationResult(
            "Dung lượng ảnh tối đa là 5MB.",
            new[] { nameof(Thumbnail) });
    }

    // Kiểm tra Category
    if (CategoryId <= 0)
    {
        yield return new ValidationResult(
            "Danh mục không hợp lệ.",
            new[] { nameof(CategoryId) });
    }
}
}
