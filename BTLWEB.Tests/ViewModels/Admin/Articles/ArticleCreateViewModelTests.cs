using System.ComponentModel.DataAnnotations;
using BTLWEB.Models;
using BTLWEB.ViewModels;

namespace BTLWEB.Tests.ViewModels.Admin.Articles;

public class ArticleCreateViewModelTests
{
    [Fact]
    public void Validate_ShouldRejectWhitespaceTitle()
    {
        var model = CreateValidModel();
        model.Title = "   ";

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ArticleCreateViewModel.Title)));
    }

    [Fact]
    public void Validate_ShouldRejectHtmlWithoutPlainTextContent()
    {
        var model = CreateValidModel();
        model.Content = "<p>&nbsp;</p><div> </div>";

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ArticleCreateViewModel.Content)));
    }

    [Fact]
    public void Validate_ShouldRejectFeaturedDraft()
    {
        var model = CreateValidModel();
        model.Status = PostStatus.Draft;
        model.IsFeatured = true;

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(ArticleCreateViewModel.IsFeatured)));
    }

    [Fact]
    public void Validate_ShouldAllowFeaturedPublishedArticle()
    {
        var model = CreateValidModel();
        model.Status = PostStatus.Published;
        model.IsFeatured = true;

        var results = Validate(model);

        Assert.Empty(results);
    }

    private static ArticleCreateViewModel CreateValidModel()
    {
        return new ArticleCreateViewModel
        {
            Title = "Triển lãm ảnh nghệ thuật Việt Nam năm 2026",
            Summary = "Mô tả ngắn cho bài viết.",
            Content = "<p>Nội dung có chữ hợp lệ.</p>",
            CategoryId = 1,
            Status = PostStatus.Draft
        };
    }

    private static List<ValidationResult> Validate(ArticleCreateViewModel model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }
}
