using BTLWEB.Models;
using BTLWEB.Repositories.Interfaces;
using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;

namespace BTLWEB.Services;

public class ArticleService : IArticleService
{
    private readonly IAdminPostRepository _postRepository;
    private readonly ISlugService _slugService;
    private readonly IFileUploadService _fileUploadService;
    private readonly IHtmlSanitizerService _htmlSanitizerService;
    private readonly ILogger<ArticleService> _logger;

    public ArticleService(
        IAdminPostRepository postRepository,
        ISlugService slugService,
        IFileUploadService fileUploadService,
        IHtmlSanitizerService htmlSanitizerService,
        ILogger<ArticleService> logger)
    {
        _postRepository = postRepository;
        _slugService = slugService;
        _fileUploadService = fileUploadService;
        _htmlSanitizerService = htmlSanitizerService;
        _logger = logger;
    }

    public async Task<ArticleListViewModel> GetAdminListAsync(ArticleFilterViewModel filter)
    {
        filter.Page = Math.Max(filter.Page, 1);
        filter.PageSize = Math.Clamp(filter.PageSize, 1, 50);
        filter.Search = string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim();
        filter.Status = PostStatus.All.Contains(filter.Status) ? filter.Status : null;
        filter.CategoryId = filter.CategoryId is > 0 ? filter.CategoryId : null;

        var result = await _postRepository.GetAdminArticlesAsync(filter);

        return new ArticleListViewModel
        {
            Filter = filter,
            Articles = new PagedResult<ArticleListItemViewModel>
            {
                Items = result.Items.Select(MapToListItem).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems
            },
            Categories = await _postRepository.GetCategoryOptionsAsync(),
            Statuses = BuildStatusOptions()
        };
    }

    public async Task<ArticleCreateViewModel> BuildCreateViewModelAsync(ArticleCreateViewModel? model = null)
    {
        model ??= new ArticleCreateViewModel();
        await PopulateFormOptionsAsync(model);
        return model;
    }

    public async Task<ArticleEditViewModel?> BuildEditViewModelAsync(int id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post is null)
        {
            return null;
        }

        var model = new ArticleEditViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Summary = post.Summary,
            Content = post.Content ?? string.Empty,
            CategoryId = post.CategoryId,
            ExistingThumbnailUrl = post.ThumbnailUrl,
            IsFeatured = post.IsFeatured,
            Status = post.Status,
            PublishedAt = post.PublishedAt,
            MetaTitle = post.MetaTitle,
            MetaDescription = post.MetaDescription
        };

        await PopulateFormOptionsAsync(model);
        return model;
    }

    public async Task<ArticleEditViewModel> BuildEditViewModelAsync(ArticleEditViewModel model)
    {
        await PopulateFormOptionsAsync(model);
        return model;
    }

    public async Task<OperationResult<int>> CreateAsync(ArticleCreateViewModel model, int authorId, CancellationToken cancellationToken = default)
    {
        if (authorId <= 0)
        {
            return OperationResult<int>.Failure("Không xác định được tác giả bài viết.");
        }

        var validationResult = await ValidateArticleRequestAsync(model);
        if (!validationResult.Succeeded)
        {
            return OperationResult<int>.Failure(validationResult.Message);
        }

        string? uploadedThumbnailUrl = null;
        if (model.Thumbnail is not null)
        {
            var uploadResult = await _fileUploadService.UploadArticleThumbnailAsync(model.Thumbnail, cancellationToken);
            if (!uploadResult.Succeeded || string.IsNullOrWhiteSpace(uploadResult.Data))
            {
                return OperationResult<int>.Failure(uploadResult.Message);
            }

            uploadedThumbnailUrl = uploadResult.Data;
        }

        var now = DateTime.UtcNow;
        var status = model.Status;
        var publishedAt = status == PostStatus.Published
            ? model.PublishedAt ?? now
            : model.PublishedAt;

        var post = new Post
        {
            Title = model.Title.Trim(),
            Slug = await _slugService.GenerateUniqueSlugAsync(
                model.Title,
                candidate => _postRepository.SlugExistsAsync(candidate)),
            Summary = model.Summary.Trim(),
            Content = _htmlSanitizerService.Sanitize(model.Content),
            ThumbnailUrl = uploadedThumbnailUrl,
            CategoryId = model.CategoryId,
            AuthorId = authorId,
            Status = status,
            IsFeatured = status == PostStatus.Published && model.IsFeatured,
            PublishedAt = publishedAt,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            ViewCount = 0,
            MetaTitle = string.IsNullOrWhiteSpace(model.MetaTitle) ? null : model.MetaTitle.Trim(),
            MetaDescription = string.IsNullOrWhiteSpace(model.MetaDescription) ? null : model.MetaDescription.Trim(),
            IsDeleted = false
        };

        try
        {
            await _postRepository.AddAsync(post);
            return OperationResult<int>.Success(post.Id, "Đã tạo bài viết.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Khong the tao bai viet {Title}.", model.Title);
            _fileUploadService.DeleteUploadedFile(uploadedThumbnailUrl);
            return OperationResult<int>.Failure("Không thể tạo bài viết. Vui lòng thử lại.");
        }
    }

    public async Task<OperationResult> UpdateAsync(ArticleEditViewModel model, CancellationToken cancellationToken = default)
    {
        var existingPost = await _postRepository.GetByIdAsync(model.Id);
        if (existingPost is null)
        {
            return OperationResult.Failure("Bài viết không tồn tại hoặc đã bị xóa.");
        }

        var validationResult = await ValidateArticleRequestAsync(model);
        if (!validationResult.Succeeded)
        {
            return OperationResult.Failure(validationResult.Message);
        }

        string? uploadedThumbnailUrl = null;
        if (model.Thumbnail is not null)
        {
            var uploadResult = await _fileUploadService.UploadArticleThumbnailAsync(model.Thumbnail, cancellationToken);
            if (!uploadResult.Succeeded || string.IsNullOrWhiteSpace(uploadResult.Data))
            {
                return OperationResult.Failure(uploadResult.Message);
            }

            uploadedThumbnailUrl = uploadResult.Data;
        }

        var oldThumbnailUrl = existingPost.ThumbnailUrl;
        existingPost.Title = model.Title.Trim();
        existingPost.Slug = await _slugService.GenerateUniqueSlugAsync(
            model.Title,
            candidate => _postRepository.SlugExistsAsync(candidate, model.Id));
        existingPost.Summary = model.Summary.Trim();
        existingPost.Content = _htmlSanitizerService.Sanitize(model.Content);
        existingPost.ThumbnailUrl = uploadedThumbnailUrl ?? existingPost.ThumbnailUrl;
        existingPost.CategoryId = model.CategoryId;
        existingPost.Status = model.Status;
        existingPost.IsFeatured = model.Status == PostStatus.Published && model.IsFeatured;
        existingPost.PublishedAt = model.Status == PostStatus.Published
            ? model.PublishedAt ?? DateTime.UtcNow
            : model.PublishedAt;
        existingPost.UpdatedAtUtc = DateTime.UtcNow;
        existingPost.MetaTitle = string.IsNullOrWhiteSpace(model.MetaTitle) ? null : model.MetaTitle.Trim();
        existingPost.MetaDescription = string.IsNullOrWhiteSpace(model.MetaDescription) ? null : model.MetaDescription.Trim();
        existingPost.Category = null;
        existingPost.Author = null;
        existingPost.DeletedByUser = null;

        try
        {
            await _postRepository.UpdateAsync(existingPost);
            if (uploadedThumbnailUrl is not null && !string.Equals(oldThumbnailUrl, uploadedThumbnailUrl, StringComparison.OrdinalIgnoreCase))
            {
                _fileUploadService.DeleteUploadedFile(oldThumbnailUrl);
            }

            return OperationResult.Success("Đã cập nhật bài viết.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Khong the cap nhat bai viet {ArticleId}.", model.Id);
            _fileUploadService.DeleteUploadedFile(uploadedThumbnailUrl);
            return OperationResult.Failure("Không thể cập nhật bài viết. Vui lòng thử lại.");
        }
    }

    public async Task<OperationResult> ChangeStatusAsync(int id, string status)
    {
        if (!PostStatus.All.Contains(status))
        {
            return OperationResult.Failure("Trạng thái bài viết không hợp lệ.");
        }

        var post = await _postRepository.GetByIdAsync(id);
        if (post is null)
        {
            return OperationResult.Failure("Bài viết không tồn tại hoặc đã bị xóa.");
        }

        post.Status = status;
        post.UpdatedAtUtc = DateTime.UtcNow;
        post.Category = null;
        post.Author = null;
        post.DeletedByUser = null;

        if (status == PostStatus.Published)
        {
            post.PublishedAt ??= DateTime.UtcNow;
        }
        else
        {
            post.IsFeatured = false;
        }

        try
        {
            await _postRepository.UpdateAsync(post);
            return OperationResult.Success("Đã cập nhật trạng thái bài viết.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Khong the doi trang thai bai viet {ArticleId}.", id);
            return OperationResult.Failure("Không thể cập nhật trạng thái bài viết. Vui lòng thử lại.");
        }
    }

    public async Task<OperationResult> SoftDeleteAsync(int id, int deletedByUserId)
    {
        if (deletedByUserId <= 0)
        {
            return OperationResult.Failure("Không xác định được người thực hiện thao tác.");
        }

        var post = await _postRepository.GetByIdAsync(id);
        if (post is null)
        {
            return OperationResult.Failure("Bài viết không tồn tại hoặc đã bị xóa.");
        }

        post.IsDeleted = true;
        post.DeletedAtUtc = DateTime.UtcNow;
        post.DeletedByUserId = deletedByUserId;
        post.UpdatedAtUtc = DateTime.UtcNow;
        post.IsFeatured = false;
        post.Category = null;
        post.Author = null;
        post.DeletedByUser = null;

        try
        {
            await _postRepository.UpdateAsync(post);
            return OperationResult.Success("Đã xóa mềm bài viết.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Khong the xoa mem bai viet {ArticleId}.", id);
            return OperationResult.Failure("Không thể xóa bài viết. Vui lòng thử lại.");
        }
    }

    private async Task<OperationResult> ValidateArticleRequestAsync(ArticleCreateViewModel model)
    {
        if (!PostStatus.All.Contains(model.Status))
        {
            return OperationResult.Failure("Trạng thái bài viết không hợp lệ.");
        }

        if (model.IsFeatured && model.Status != PostStatus.Published)
        {
            return OperationResult.Failure("Chỉ bài đã xuất bản mới được đánh dấu nổi bật.");
        }

        if (!await _postRepository.CategoryExistsAsync(model.CategoryId))
        {
            return OperationResult.Failure("Danh mục bài viết không tồn tại.");
        }

        return OperationResult.Success();
    }

    private async Task PopulateFormOptionsAsync(ArticleCreateViewModel model)
    {
        model.Categories = await _postRepository.GetCategoryOptionsAsync();
        model.Statuses = BuildStatusOptions();
    }

    private static ArticleListItemViewModel MapToListItem(Post post)
    {
        return new ArticleListItemViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            ThumbnailUrl = post.ThumbnailUrl,
            CategoryName = post.Category?.Name ?? "Chưa phân loại",
            AuthorName = post.Author?.FullName ?? post.Author?.Username,
            Status = post.Status,
            IsFeatured = post.IsFeatured,
            CreatedAtUtc = post.CreatedAtUtc,
            PublishedAt = post.PublishedAt,
            ViewCount = post.ViewCount
        };
    }

    private static IReadOnlyList<ArticleStatusOptionViewModel> BuildStatusOptions()
    {
        return
        [
            new ArticleStatusOptionViewModel { Value = PostStatus.Draft, Text = "Nháp" },
            new ArticleStatusOptionViewModel { Value = PostStatus.Published, Text = "Xuất bản" },
            new ArticleStatusOptionViewModel { Value = PostStatus.Hidden, Text = "Ẩn" },
            new ArticleStatusOptionViewModel { Value = PostStatus.Archived, Text = "Lưu trữ" }
        ];
    }
}
