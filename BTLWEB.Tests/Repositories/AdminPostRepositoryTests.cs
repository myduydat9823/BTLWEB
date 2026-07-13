using BTLWEB.Data;
using BTLWEB.Models;
using BTLWEB.Repositories;
using BTLWEB.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Tests.Repositories;

public class AdminPostRepositoryTests
{
    [Fact]
    public async Task GetAdminArticlesAsync_ShouldSearchByTitle()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);
        var repository = new AdminPostRepository(dbContext);

        var result = await repository.GetAdminArticlesAsync(new ArticleFilterViewModel { Search = "Triển lãm" });

        Assert.Single(result.Items);
        Assert.Equal("trien-lam-anh", result.Items[0].Slug);
    }

    [Fact]
    public async Task GetAdminArticlesAsync_ShouldFilterByCategoryAndStatus()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);
        var repository = new AdminPostRepository(dbContext);

        var result = await repository.GetAdminArticlesAsync(new ArticleFilterViewModel
        {
            CategoryId = 2,
            Status = PostStatus.Draft
        });

        Assert.Single(result.Items);
        Assert.Equal("draft-category-2", result.Items[0].Slug);
    }

    [Fact]
    public async Task GetAdminArticlesAsync_ShouldPaginate()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);
        var repository = new AdminPostRepository(dbContext);

        var result = await repository.GetAdminArticlesAsync(new ArticleFilterViewModel
        {
            Page = 2,
            PageSize = 2
        });

        Assert.Equal(4, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task GetAdminArticlesAsync_ShouldExcludeSoftDeletedArticles()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);
        var repository = new AdminPostRepository(dbContext);

        var result = await repository.GetAdminArticlesAsync(new ArticleFilterViewModel { Search = "Đã xóa" });

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetPublishedArticlesAsync_ShouldExcludeHiddenDeletedAndFutureArticles()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);
        var repository = new AdminPostRepository(dbContext);

        var result = await repository.GetPublishedArticlesAsync(10);

        Assert.Single(result);
        Assert.Equal("trien-lam-anh", result[0].Slug);
    }

    [Fact]
    public async Task SlugExistsAsync_ShouldIgnoreExcludedPost()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);
        var repository = new AdminPostRepository(dbContext);

        var existsForCurrentPost = await repository.SlugExistsAsync("trien-lam-anh", excludedPostId: 1);
        var existsForOtherPost = await repository.SlugExistsAsync("trien-lam-anh", excludedPostId: 2);

        Assert.False(existsForCurrentPost);
        Assert.True(existsForOtherPost);
    }

    [Fact]
    public async Task CategoryExistsAsync_ShouldReturnExpectedResult()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext);
        var repository = new AdminPostRepository(dbContext);

        Assert.True(await repository.CategoryExistsAsync(1));
        Assert.False(await repository.CategoryExistsAsync(999));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task SeedAsync(AppDbContext dbContext)
    {
        var now = DateTime.UtcNow;
        var categories = new[]
        {
            new Category { Id = 1, Name = "Tin tức", Slug = "tin-tuc" },
            new Category { Id = 2, Name = "Ảnh nổi bật", Slug = "anh-noi-bat" }
        };
        var role = new Role { RoleId = 1, RoleName = "Admin", DisplayName = "Quản trị viên" };
        var author = new User
        {
            UserId = 1,
            RoleId = 1,
            Username = "admin",
            NormalizedUsername = "ADMIN",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            PasswordHash = "hash",
            FullName = "Admin",
            Role = role
        };

        await dbContext.Categories.AddRangeAsync(categories);
        await dbContext.Roles.AddAsync(role);
        await dbContext.Users.AddAsync(author);
        await dbContext.Posts.AddRangeAsync(
            CreatePost(1, "trien-lam-anh", "Triển lãm ảnh", 1, author.UserId, PostStatus.Published, now.AddDays(-2), now.AddDays(-3)),
            CreatePost(2, "draft-category-2", "Bài nháp", 2, author.UserId, PostStatus.Draft, null, now.AddDays(-2)),
            CreatePost(3, "hidden-post", "Bài ẩn", 1, author.UserId, PostStatus.Hidden, now.AddDays(-1), now.AddDays(-1)),
            CreatePost(4, "future-post", "Bài hẹn giờ", 1, author.UserId, PostStatus.Published, now.AddDays(2), now),
            CreatePost(5, "deleted-post", "Đã xóa", 1, author.UserId, PostStatus.Published, now.AddDays(-1), now.AddDays(-4), isDeleted: true));

        await dbContext.SaveChangesAsync();
    }

    private static Post CreatePost(
        int id,
        string slug,
        string title,
        int categoryId,
        int authorId,
        string status,
        DateTime? publishedAt,
        DateTime createdAtUtc,
        bool isDeleted = false)
    {
        return new Post
        {
            Id = id,
            Slug = slug,
            Title = title,
            Summary = "Summary",
            Content = "Content",
            CategoryId = categoryId,
            AuthorId = authorId,
            Status = status,
            PublishedAt = publishedAt,
            CreatedAtUtc = createdAtUtc,
            IsDeleted = isDeleted
        };
    }
}
