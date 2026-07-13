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

    public Task<ArticleListViewModel> GetAdminListAsync(ArticleFilterViewModel filter)
    {
        return BuildListAsync(filter, includeDeleted: false);
    }

    public Task<ArticleListViewModel> GetDeletedListAsync(ArticleFilterViewModel filter)
    {
        return BuildListAsync(filter, includeDeleted: true);
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

    public async Task<ArticlePreviewViewModel?> BuildPreviewViewModelAsync(int id, bool includeDeleted = false)
    {
        var post = includeDeleted
            ? await _postRepository.GetDeletedByIdAsync(id)
            : await _postRepository.GetByIdAsync(id);

        if (post is null)
        {
            return null;
        }

        var logs = await _postRepository.GetLogsByPostIdAsync(post.Id);
        return new ArticlePreviewViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Summary = post.Summary,
            Content = post.Content ?? string.Empty,
            ThumbnailUrl = post.ThumbnailUrl,
            CategoryName = post.Category?.Name ?? "Chưa phân loại",
            AuthorName = post.Author?.FullName ?? post.Author?.Username,
            Status = post.Status,
            IsFeatured = post.IsFeatured,
            CreatedAtUtc = post.CreatedAtUtc,
            UpdatedAtUtc = post.UpdatedAtUtc,
            PublishedAt = post.PublishedAt,
            MetaTitle = post.MetaTitle,
            MetaDescription = post.MetaDescription,
            Logs = logs.Select(MapToLogItem).ToList()
        };
    }

    public async Task<OperationResult<int>> CreateAsync(
        ArticleCreateViewModel model,
        int authorId,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
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
        var publishedAt = PostStatus.IsVisibleStatus(status)
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
            IsFeatured = PostStatus.IsVisibleStatus(status) && model.IsFeatured,
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
            await AddLogSafeAsync(post, ArticleAdminAction.Create, authorId, null, post.Status, ipAddress, userAgent);
            return OperationResult<int>.Success(post.Id, "Đã tạo bài viết.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Khong the tao bai viet {Title}.", model.Title);
            _fileUploadService.DeleteUploadedFile(uploadedThumbnailUrl);
            return OperationResult<int>.Failure("Không thể tạo bài viết. Vui lòng thử lại.");
        }
    }

    public async Task<OperationResult> UpdateAsync(
        ArticleEditViewModel model,
        int actorUserId,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        if (actorUserId <= 0)
        {
            return OperationResult.Failure("Không xác định được người thực hiện thao tác.");
        }

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

        var oldStatus = existingPost.Status;
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
        existingPost.IsFeatured = PostStatus.IsVisibleStatus(model.Status) && model.IsFeatured;
        existingPost.PublishedAt = PostStatus.IsVisibleStatus(model.Status)
            ? model.PublishedAt ?? DateTime.UtcNow
            : model.PublishedAt;
        existingPost.UpdatedAtUtc = DateTime.UtcNow;
        existingPost.MetaTitle = string.IsNullOrWhiteSpace(model.MetaTitle) ? null : model.MetaTitle.Trim();
        existingPost.MetaDescription = string.IsNullOrWhiteSpace(model.MetaDescription) ? null : model.MetaDescription.Trim();
        ClearNavigationProperties(existingPost);

        try
        {
            await _postRepository.UpdateAsync(existingPost);
            if (uploadedThumbnailUrl is not null && !string.Equals(oldThumbnailUrl, uploadedThumbnailUrl, StringComparison.OrdinalIgnoreCase))
            {
                _fileUploadService.DeleteUploadedFile(oldThumbnailUrl);
            }

            await AddLogSafeAsync(existingPost, ArticleAdminAction.Update, actorUserId, oldStatus, existingPost.Status, ipAddress, userAgent);
            return OperationResult.Success("Đã cập nhật bài viết.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Khong the cap nhat bai viet {ArticleId}.", model.Id);
            _fileUploadService.DeleteUploadedFile(uploadedThumbnailUrl);
            return OperationResult.Failure("Không thể cập nhật bài viết. Vui lòng thử lại.");
        }
    }

    public async Task<OperationResult> ChangeStatusAsync(int id, string status, int actorUserId, string? ipAddress = null, string? userAgent = null)
    {
        if (actorUserId <= 0)
        {
            return OperationResult.Failure("Không xác định được người thực hiện thao tác.");
        }

        if (!PostStatus.All.Contains(status))
        {
            return OperationResult.Failure("Trạng thái bài viết không hợp lệ.");
        }

        var post = await _postRepository.GetByIdAsync(id);
        if (post is null)
        {
            return OperationResult.Failure("Bài viết không tồn tại hoặc đã bị xóa.");
        }

        var oldStatus = post.Status;
        post.Status = status;
        post.UpdatedAtUtc = DateTime.UtcNow;
        ClearNavigationProperties(post);

        if (PostStatus.IsVisibleStatus(status))
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
            await AddLogSafeAsync(post, ArticleAdminAction.ChangeStatus, actorUserId, oldStatus, status, ipAddress, userAgent);
            return OperationResult.Success("Đã cập nhật trạng thái bài viết.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Khong the doi trang thai bai viet {ArticleId}.", id);
            return OperationResult.Failure("Không thể cập nhật trạng thái bài viết. Vui lòng thử lại.");
        }
    }

    public async Task<OperationResult> SoftDeleteAsync(int id, int deletedByUserId, string? ipAddress = null, string? userAgent = null)
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

        var oldStatus = post.Status;
        post.IsDeleted = true;
        post.DeletedAtUtc = DateTime.UtcNow;
        post.DeletedByUserId = deletedByUserId;
        post.UpdatedAtUtc = DateTime.UtcNow;
        post.IsFeatured = false;
        ClearNavigationProperties(post);

        try
        {
            await _postRepository.UpdateAsync(post);
            await AddLogSafeAsync(post, ArticleAdminAction.SoftDelete, deletedByUserId, oldStatus, post.Status, ipAddress, userAgent);
            return OperationResult.Success("Đã xóa mềm bài viết.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Khong the xoa mem bai viet {ArticleId}.", id);
            return OperationResult.Failure("Không thể xóa bài viết. Vui lòng thử lại.");
        }
    }

    public async Task<OperationResult> RestoreAsync(int id, int actorUserId, string? ipAddress = null, string? userAgent = null)
    {
        if (actorUserId <= 0)
        {
            return OperationResult.Failure("Không xác định được người thực hiện thao tác.");
        }

        var post = await _postRepository.GetDeletedByIdAsync(id);
        if (post is null)
        {
            return OperationResult.Failure("Bài viết không tồn tại hoặc chưa bị xóa.");
        }

        var oldStatus = post.Status;
        post.IsDeleted = false;
        post.DeletedAtUtc = null;
        post.DeletedByUserId = null;
        post.UpdatedAtUtc = DateTime.UtcNow;
        ClearNavigationProperties(post);

        try
        {
            await _postRepository.UpdateAsync(post);
            await AddLogSafeAsync(post, ArticleAdminAction.Restore, actorUserId, oldStatus, post.Status, ipAddress, userAgent);
            return OperationResult.Success("Đã khôi phục bài viết.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Khong the khoi phuc bai viet {ArticleId}.", id);
            return OperationResult.Failure("Không thể khôi phục bài viết. Vui lòng thử lại.");
        }
    }

    private async Task<ArticleListViewModel> BuildListAsync(ArticleFilterViewModel filter, bool includeDeleted)
    {
        filter.Page = Math.Max(filter.Page, 1);
        filter.PageSize = Math.Clamp(filter.PageSize, 1, 50);
        filter.Search = string.IsNullOrWhiteSpace(filter.Search) ? null : filter.Search.Trim();
        filter.Status = PostStatus.All.Contains(filter.Status) ? filter.Status : null;
        filter.CategoryId = filter.CategoryId is > 0 ? filter.CategoryId : null;

        var result = includeDeleted
            ? await _postRepository.GetDeletedArticlesAsync(filter)
            : await _postRepository.GetAdminArticlesAsync(filter);

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

    private async Task<OperationResult> ValidateArticleRequestAsync(ArticleCreateViewModel model)
    {
        if (!PostStatus.IsReviewStatus(model.Status) && !PostStatus.All.Contains(model.Status))
        {
            return OperationResult.Failure("Trạng thái bài viết không hợp lệ.");
        }

        if (model.IsFeatured && !PostStatus.IsVisibleStatus(model.Status))
        {
            return OperationResult.Failure("Chỉ bài đã được duyệt mới được đánh dấu nổi bật.");
        }

        if (!await _postRepository.CategoryExistsAsync(model.CategoryId))
        {
            return OperationResult.Failure("Danh mục bài viết không tồn tại.");
        }

        if (model.Thumbnail is not null)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (string.IsNullOrWhiteSpace(model.Thumbnail.ContentType) || !allowedTypes.Contains(model.Thumbnail.ContentType.Trim().ToLowerInvariant()))
            {
                return OperationResult.Failure("Định dạng ảnh không hợp lệ. Vui lòng tải lên file JPG, PNG hoặc WEBP.");
            }

            const long maxBytes = 5 * 1024 * 1024;
            if (model.Thumbnail.Length > maxBytes)
            {
                return OperationResult.Failure("Kích thước ảnh quá lớn. Vui lòng tải ảnh có dung lượng ≤ 5 MB.");
            }
        }

        return OperationResult.Success();
    }

    private async Task PopulateFormOptionsAsync(ArticleCreateViewModel model)
    {
        model.Categories = await _postRepository.GetCategoryOptionsAsync();
        model.Statuses = BuildStatusOptions();
    }

    private async Task AddLogSafeAsync(
        Post post,
        string action,
        int actorUserId,
        string? statusBefore,
        string? statusAfter,
        string? ipAddress,
        string? userAgent)
    {
        try
        {
            await _postRepository.AddLogAsync(new ArticleAdminLog
            {
                PostId = post.Id,
                ActorUserId = actorUserId,
                Action = action,
                StatusBefore = statusBefore,
                StatusAfter = statusAfter,
                TitleSnapshot = post.Title,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Khong the ghi nhat ky thao tac bai viet {ArticleId}.", post.Id);
        }
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
            DeletedAtUtc = post.DeletedAtUtc,
            DeletedByUserName = post.DeletedByUser?.FullName ?? post.DeletedByUser?.Username,
            ViewCount = post.ViewCount
        };
    }

    private static ArticleAdminLogItemViewModel MapToLogItem(ArticleAdminLog log)
    {
        return new ArticleAdminLogItemViewModel
        {
            Action = log.Action,
            StatusBefore = log.StatusBefore,
            StatusAfter = log.StatusAfter,
            TitleSnapshot = log.TitleSnapshot,
            ActorName = log.ActorUser?.FullName ?? log.ActorUser?.Username,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            Note = log.Note,
            CreatedAtUtc = log.CreatedAtUtc
        };
    }

    private static IReadOnlyList<ArticleStatusOptionViewModel> BuildStatusOptions()
    {
        return
        [
            new ArticleStatusOptionViewModel { Value = PostStatus.Pending, Text = "Chờ duyệt" },
            new ArticleStatusOptionViewModel { Value = PostStatus.Approved, Text = "Đã duyệt" },
            new ArticleStatusOptionViewModel { Value = PostStatus.Rejected, Text = "Từ chối" }
        ];
    }

    private static void ClearNavigationProperties(Post post)
    {
        post.Category = null;
        post.Author = null;
        post.DeletedByUser = null;
    }
}
