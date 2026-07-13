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
        Assert.Equal("/uploads/articles/test.jpg", uploadService.DeletedPath);
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

        public Task UpdateAsync(Post post) => Task.CompletedTask;

        public Task<PagedResult<Post>> GetAdminArticlesAsync(ArticleFilterViewModel filter)
        {
            return Task.FromResult(new PagedResult<Post>());
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
        public string? DeletedPath { get; private set; }

        public Task<OperationResult<string>> UploadArticleThumbnailAsync(IFormFile? file, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(UploadResult);
        }

        public void DeleteUploadedFile(string? relativePath)
        {
            DeletedPath = relativePath;
        }
    }

    private sealed class FakeHtmlSanitizerService : IHtmlSanitizerService
    {
        public string Sanitize(string? html) => "sanitized-content";
    }
}
