using BTLWEB.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace BTLWEB.Tests.Services;

public class FileUploadServiceTests : IDisposable
{
    private readonly string _webRootPath;
    private readonly FileUploadService _fileUploadService;

    public FileUploadServiceTests()
    {
        _webRootPath = Path.Combine(Path.GetTempPath(), $"btlweb-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_webRootPath);
        _fileUploadService = new FileUploadService(
            new TestWebHostEnvironment(_webRootPath),
            LoggerFactory.Create(_ => { }).CreateLogger<FileUploadService>());
    }

    [Fact]
    public async Task UploadArticleThumbnailAsync_ShouldSaveValidFileWithSafeGeneratedName()
    {
        var file = CreateFile("../../../dangerous<script>.jpg", "image/jpeg", [1, 2, 3]);

        var result = await _fileUploadService.UploadArticleThumbnailAsync(file);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.StartsWith("/uploads/articles/", result.Data);
        Assert.EndsWith(".jpg", result.Data);
        Assert.DoesNotContain("dangerous", result.Data);
        Assert.True(File.Exists(ToPhysicalPath(result.Data)));
    }

    [Fact]
    public async Task UploadArticleThumbnailAsync_ShouldRejectInvalidExtension()
    {
        var file = CreateFile("shell.exe", "application/octet-stream", [1, 2, 3]);

        var result = await _fileUploadService.UploadArticleThumbnailAsync(file);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UploadArticleThumbnailAsync_ShouldRejectInvalidMimeType()
    {
        var file = CreateFile("photo.jpg", "application/octet-stream", [1, 2, 3]);

        var result = await _fileUploadService.UploadArticleThumbnailAsync(file);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UploadArticleThumbnailAsync_ShouldRejectFileOverFiveMegabytes()
    {
        var bytes = new byte[(5 * 1024 * 1024) + 1];
        var file = CreateFile("photo.png", "image/png", bytes);

        var result = await _fileUploadService.UploadArticleThumbnailAsync(file);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task UploadArticleThumbnailAsync_ShouldRejectEmptyFile()
    {
        var file = CreateFile("photo.webp", "image/webp", []);

        var result = await _fileUploadService.UploadArticleThumbnailAsync(file);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task DeleteUploadedFile_ShouldRemoveArticleUpload()
    {
        var file = CreateFile("photo.webp", "image/webp", [1, 2, 3]);
        var result = await _fileUploadService.UploadArticleThumbnailAsync(file);

        _fileUploadService.DeleteUploadedFile(result.Data);

        Assert.False(File.Exists(ToPhysicalPath(result.Data!)));
    }

    public void Dispose()
    {
        if (Directory.Exists(_webRootPath))
        {
            Directory.Delete(_webRootPath, recursive: true);
        }
    }

    private string ToPhysicalPath(string relativePath)
    {
        var fileName = Path.GetFileName(relativePath);
        return Path.Combine(_webRootPath, "uploads", "articles", fileName);
    }

    private static IFormFile CreateFile(string fileName, string contentType, byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment(string webRootPath)
        {
            WebRootPath = webRootPath;
            WebRootFileProvider = new PhysicalFileProvider(webRootPath);
            ContentRootPath = webRootPath;
            ContentRootFileProvider = new PhysicalFileProvider(webRootPath);
        }

        public string ApplicationName { get; set; } = "BTLWEB.Tests";
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Development";
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
    }
}
