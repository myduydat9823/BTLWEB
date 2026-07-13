using BTLWEB.Models;
using BTLWEB.Repositories.Interfaces;
using BTLWEB.Services;
using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace BTLWEB.Tests.Services;

public class ArticleServiceTests
{
    [Fact]
    public async Task BuildCreateViewModelAsync_ShouldPopulateCategoriesAndStatuses()
    {
        var repository = new FakeAdminPostRepository();
        var service = CreateService(repository);

        var model = await service.BuildCreateViewModelAsync();

        Assert.NotEmpty(model.Categories);
        Assert.Equal(PostStatus.All.Length, model.Statuses.Count);
    }

    [Fact]
    public async Task GetAdminListAsync_ShouldMapPagedArticlesAndNormalizeFilters()
    {
        var repository = new FakeAdminPostRepository
        {
            AdminArticlesResult = new PagedResult<Post>
            {
                Items =
                [
                    new Post
                    {
                        Id = 9,
                        Title = "Triển lãm ảnh",
                        Slug = "trien-lam-anh",
                        ThumbnailUrl = "/uploads/articles/test.jpg",
                        Category = new Category { Name = "Tin tức" },
                        Author = new User { FullName = "Quản trị viên" },
                        Status = PostStatus.Published,
                        IsFeatured = true,
                        CreatedAtUtc = new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc),
                        PublishedAt = new DateTime(2026, 7, 14, 0, 0, 0, DateTimeKind.Utc),
                        ViewCount = 25
                    }
                ],
                Page = 2,
                PageSize = 5,
                TotalItems = 11
            }
        };
        var service = CreateService(repository);

        var model = await service.GetAdminListAsync(new ArticleFilterViewModel
        {
            Search = "  Triển lãm  ",
            CategoryId = 1,
            Status = PostStatus.Published,
            IsFeatured = true,
            Page = 2,
            PageSize = 5
        });

        Assert.Equal("Triển lãm", repository.LastFilter?.Search);
        Assert.Equal(2, model.Articles.Page);
        Assert.Equal(11, model.Articles.TotalItems);
        Assert.Equal(PostStatus.All.Length, model.Statuses.Count);

        var article = Assert.Single(model.Articles.Items);
        Assert.Equal(9, article.Id);
        Assert.Equal("Tin tức", article.CategoryName);
        Assert.Equal("Quản trị viên", article.AuthorName);
        Assert.True(article.IsFeatured);
    }

    [Fact]
    public async Task BuildEditViewModelAsync_ShouldMapExistingPost()
    {
        var repository = new FakeAdminPostRepository();
        repository.Posts.Add(CreateExistingPost());
        var service = CreateService(repository);

        var model = await service.BuildEditViewModelAsync(4);

        Assert.NotNull(model);
        Assert.Equal("Bài viết cũ", model.Title);
        Assert.Equal("bai-viet-cu", model.Slug);
        Assert.Equal("/uploads/articles/old.jpg", model.ExistingThumbnailUrl);
        Assert.NotEmpty(model.Categories);
        Assert.Equal(PostStatus.All.Length, model.Statuses.Count);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateDraftArticleWithServerSideFields()
    {
        var repository = new FakeAdminPostRepository();
        var sanitizer = new FakeHtmlSanitizerService();
        var service = CreateService(repository, htmlSanitizerService: sanitizer);
        var model = CreateValidModel();

        var result = await service.CreateAsync(model, authorId: 7);

        Assert.True(result.Succeeded);
        var post = Assert.Single(repository.Posts);
        Assert.Equal(7, post.AuthorId);
        Assert.Equal("trien-lam-anh", post.Slug);
        Assert.Equal("sanitized-content", post.Content);
        Assert.Equal(PostStatus.Draft, post.Status);
        Assert.False(post.IsDeleted);
        Assert.False(post.IsFeatured);
        Assert.Equal(0, post.ViewCount);
        Assert.NotEqual(default, post.CreatedAtUtc);
        Assert.NotNull(post.UpdatedAtUtc);
    }

    [Fact]
    public async Task CreateAsync_ShouldSetPublishedAtWhenPublishingWithoutDate()
    {
        var repository = new FakeAdminPostRepository();
        var service = CreateService(repository);
        var model = CreateValidModel();
        model.Status = PostStatus.Published;
        model.IsFeatured = true;
        model.PublishedAt = null;

        var result = await service.CreateAsync(model, authorId: 7);

        Assert.True(result.Succeeded);
        var post = Assert.Single(repository.Posts);
        Assert.True(post.IsFeatured);
        Assert.NotNull(post.PublishedAt);
    }

    [Fact]
    public async Task CreateAsync_ShouldUseUniqueSlug()
    {
        var repository = new FakeAdminPostRepository
        {
            ExistingSlugs = ["trien-lam-anh", "trien-lam-anh-2"]
        };
        var service = CreateService(repository);

        var result = await service.CreateAsync(CreateValidModel(), authorId: 7);

        Assert.True(result.Succeeded);
        Assert.Equal("trien-lam-anh-3", repository.Posts.Single().Slug);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectMissingCategory()
    {
        var repository = new FakeAdminPostRepository { CategoryExists = false };
        var service = CreateService(repository);

        var result = await service.CreateAsync(CreateValidModel(), authorId: 7);

        Assert.False(result.Succeeded);
        Assert.Empty(repository.Posts);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectFeaturedDraft()
    {
        var service = CreateService(new FakeAdminPostRepository());
        var model = CreateValidModel();
        model.IsFeatured = true;
        model.Status = PostStatus.Draft;

        var result = await service.CreateAsync(model, authorId: 7);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnUploadFailure()
    {
        var repository = new FakeAdminPostRepository();
        var uploadService = new FakeFileUploadService { UploadResult = OperationResult<string>.Failure("upload failed") };
        var service = CreateService(repository, uploadService);
        var model = CreateValidModel();
        model.Thumbnail = CreateFile();

        var result = await service.CreateAsync(model, authorId: 7);

        Assert.False(result.Succeeded);
        Assert.Empty(repository.Posts);
    }

    [Fact]
    public async Task CreateAsync_ShouldDeleteUploadedFileWhenDatabaseSaveFails()
    {
        var repository = new FakeAdminPostRepository { ThrowOnAdd = true };
        var uploadService = new FakeFileUploadService();
        var service = CreateService(repository, uploadService);
        var model = CreateValidModel();
        model.Thumbnail = CreateFile();

        var result = await service.CreateAsync(model, authorId: 7);

        Assert.False(result.Succeeded);
        Assert.Contains("/uploads/articles/test.jpg", uploadService.DeletedPaths);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEditableFieldsAndPreserveServerFields()
    {
        var repository = new FakeAdminPostRepository();
        repository.Posts.Add(CreateExistingPost());
        var uploadService = new FakeFileUploadService();
        var service = CreateService(repository, uploadService);

        var result = await service.UpdateAsync(new ArticleEditViewModel
        {
            Id = 4,
            Title = " Ảnh mới ",
            Summary = " Summary mới ",
            Content = "<script>alert(1)</script><p>Nội dung</p>",
            CategoryId = 1,
            Status = PostStatus.Published,
            IsFeatured = true,
            Thumbnail = CreateFile(),
            MetaTitle = " Meta mới ",
            MetaDescription = " Mô tả mới "
        });

        Assert.True(result.Succeeded);
        var post = Assert.Single(repository.Posts);
        Assert.Equal(4, post.Id);
        Assert.Equal(7, post.AuthorId);
        Assert.Equal(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), post.CreatedAtUtc);
        Assert.Equal(99, post.ViewCount);
        Assert.Equal("anh-moi", post.Slug);
        Assert.Equal("Ảnh mới", post.Title);
        Assert.Equal("sanitized-content", post.Content);
        Assert.Equal("/uploads/articles/test.jpg", post.ThumbnailUrl);
        Assert.True(post.IsFeatured);
        Assert.Contains("/uploads/articles/old.jpg", uploadService.DeletedPaths);
    }

    [Fact]
    public async Task UpdateAsync_ShouldDeleteNewUploadWhenDatabaseSaveFails()
    {
        var repository = new FakeAdminPostRepository { ThrowOnUpdate = true };
        repository.Posts.Add(CreateExistingPost());
        var uploadService = new FakeFileUploadService();
        var service = CreateService(repository, uploadService);

        var result = await service.UpdateAsync(new ArticleEditViewModel
        {
            Id = 4,
            Title = "Ảnh mới",
            Summary = "Summary mới",
            Content = "<p>Nội dung</p>",
            CategoryId = 1,
            Status = PostStatus.Published,
            Thumbnail = CreateFile()
        });

        Assert.False(result.Succeeded);
        Assert.Contains("/uploads/articles/test.jpg", uploadService.DeletedPaths);
    }

    [Fact]
    public async Task ChangeStatusAsync_ShouldPublishArticleAndSetPublishedAt()
    {
        var repository = new FakeAdminPostRepository();
        repository.Posts.Add(CreateExistingPost());
        var service = CreateService(repository);

        var result = await service.ChangeStatusAsync(4, PostStatus.Published);

        Assert.True(result.Succeeded);
        var post = Assert.Single(repository.Posts);
        Assert.Equal(PostStatus.Published, post.Status);
        Assert.NotNull(post.PublishedAt);
        Assert.NotNull(post.UpdatedAtUtc);
    }

    [Fact]
    public async Task ChangeStatusAsync_ShouldRemoveFeaturedFlagWhenHidingArticle()
    {
        var repository = new FakeAdminPostRepository();
        var post = CreateExistingPost();
        post.Status = PostStatus.Published;
        post.IsFeatured = true;
        repository.Posts.Add(post);
        var service = CreateService(repository);

        var result = await service.ChangeStatusAsync(4, PostStatus.Hidden);

        Assert.True(result.Succeeded);
        Assert.Equal(PostStatus.Hidden, post.Status);
        Assert.False(post.IsFeatured);
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldMarkArticleDeletedAndClearFeaturedFlag()
    {
        var repository = new FakeAdminPostRepository();
        var post = CreateExistingPost();
        post.IsFeatured = true;
        repository.Posts.Add(post);
        var service = CreateService(repository);

        var result = await service.SoftDeleteAsync(4, deletedByUserId: 11);

        Assert.True(result.Succeeded);
        Assert.True(post.IsDeleted);
        Assert.Equal(11, post.DeletedByUserId);
        Assert.NotNull(post.DeletedAtUtc);
        Assert.NotNull(post.UpdatedAtUtc);
        Assert.False(post.IsFeatured);
    }

    private static ArticleService CreateService(
        FakeAdminPostRepository repository,
        IFileUploadService? fileUploadService = null,
        IHtmlSanitizerService? htmlSanitizerService = null)
    {
        return new ArticleService(
            repository,
            new SlugService(),
            fileUploadService ?? new FakeFileUploadService(),
            htmlSanitizerService ?? new FakeHtmlSanitizerService(),
            NullLogger<ArticleService>.Instance);
    }

    private static ArticleCreateViewModel CreateValidModel()
    {
        return new ArticleCreateViewModel
        {
            Title = " Triển lãm ảnh ",
            Summary = " Summary ",
            Content = "<p>Content</p>",
            CategoryId = 1,
            Status = PostStatus.Draft,
            MetaTitle = " Meta title ",
            MetaDescription = " Meta description "
        };
    }

    private static Post CreateExistingPost()
    {
        return new Post
        {
            Id = 4,
            Title = "Bài viết cũ",
            Slug = "bai-viet-cu",
            Summary = "Summary cũ",
            Content = "<p>Nội dung cũ</p>",
            ThumbnailUrl = "/uploads/articles/old.jpg",
            CategoryId = 1,
            AuthorId = 7,
            Status = PostStatus.Draft,
            IsFeatured = false,
            CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            ViewCount = 99,
            MetaTitle = "Meta cũ",
            MetaDescription = "Mô tả cũ"
        };
    }

    private static IFormFile CreateFile()
    {
        return new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "file", "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    private sealed class FakeAdminPostRepository : IAdminPostRepository
    {
        public List<Post> Posts { get; } = [];
        public HashSet<string> ExistingSlugs { get; init; } = [];
        public bool CategoryExists { get; init; } = true;
        public bool ThrowOnAdd { get; init; }
        public bool ThrowOnUpdate { get; init; }
        public ArticleFilterViewModel? LastFilter { get; private set; }
        public PagedResult<Post> AdminArticlesResult { get; init; } = new();

        public Task<Post?> GetByIdAsync(int id) => Task.FromResult<Post?>(Posts.FirstOrDefault(x => x.Id == id));
        public Task<Post?> GetBySlugAsync(string slug) => Task.FromResult<Post?>(Posts.FirstOrDefault(x => x.Slug == slug));
        public Task<bool> SlugExistsAsync(string slug, int? excludedPostId = null) => Task.FromResult(ExistingSlugs.Contains(slug));

        public Task AddAsync(Post post)
        {
            if (ThrowOnAdd)
            {
                throw new InvalidOperationException("DB failure");
            }

            post.Id = Posts.Count + 1;
            Posts.Add(post);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Post post)
        {
            if (ThrowOnUpdate)
            {
                throw new InvalidOperationException("DB failure");
            }

            var index = Posts.FindIndex(x => x.Id == post.Id);
            if (index >= 0)
            {
                Posts[index] = post;
            }

            return Task.CompletedTask;
        }

        public Task<PagedResult<Post>> GetAdminArticlesAsync(ArticleFilterViewModel filter)
        {
            LastFilter = filter;
            return Task.FromResult(AdminArticlesResult);
        }

        public Task<List<Post>> GetPublishedArticlesAsync(int take, int? categoryId = null)
        {
            return Task.FromResult(new List<Post>());
        }

        public Task<bool> CategoryExistsAsync(int categoryId) => Task.FromResult(CategoryExists);

        public Task<List<ArticleCategoryOptionViewModel>> GetCategoryOptionsAsync()
        {
            return Task.FromResult(new List<ArticleCategoryOptionViewModel>
            {
                new() { CategoryId = 1, Name = "Tin tức" }
            });
        }
    }

    private sealed class FakeFileUploadService : IFileUploadService
    {
        public OperationResult<string> UploadResult { get; init; } = OperationResult<string>.Success("/uploads/articles/test.jpg");
        public List<string> DeletedPaths { get; } = [];

        public Task<OperationResult<string>> UploadArticleThumbnailAsync(IFormFile? file, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(UploadResult);
        }

        public void DeleteUploadedFile(string? relativePath)
        {
            if (!string.IsNullOrWhiteSpace(relativePath))
            {
                DeletedPaths.Add(relativePath);
            }
        }
    }

    private sealed class FakeHtmlSanitizerService : IHtmlSanitizerService
    {
        public string Sanitize(string? html) => "sanitized-content";
    }
}
