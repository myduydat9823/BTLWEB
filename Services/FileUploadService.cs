using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;

namespace BTLWEB.Services;

public class FileUploadService : IFileUploadService
{
    private const long MaxArticleThumbnailBytes = 5 * 1024 * 1024;
    private static readonly IReadOnlyDictionary<string, string[]> AllowedContentTypes = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = ["image/jpeg"],
        [".jpeg"] = ["image/jpeg"],
        [".png"] = ["image/png"],
        [".webp"] = ["image/webp"]
    };

    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<FileUploadService> _logger;

    public FileUploadService(IWebHostEnvironment webHostEnvironment, ILogger<FileUploadService> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    public async Task<OperationResult<string>> UploadArticleThumbnailAsync(IFormFile? file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return OperationResult<string>.Failure("Vui lòng chọn ảnh đại diện hợp lệ.");
        }

        if (file.Length > MaxArticleThumbnailBytes)
        {
            return OperationResult<string>.Failure("Ảnh đại diện bài viết tối đa 5MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedContentTypes.TryGetValue(extension, out var allowedMimeTypes))
        {
            return OperationResult<string>.Failure("Ảnh đại diện chỉ chấp nhận JPG, PNG hoặc WEBP.");
        }

        if (string.IsNullOrWhiteSpace(file.ContentType)
            || !allowedMimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return OperationResult<string>.Failure("MIME type của ảnh đại diện không hợp lệ.");
        }

        var uploadRoot = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "articles");
        Directory.CreateDirectory(uploadRoot);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadRoot, fileName);
        var relativePath = $"/uploads/articles/{fileName}";

        try
        {
            await using var stream = File.Create(physicalPath);
            await file.CopyToAsync(stream, cancellationToken);
            return OperationResult<string>.Success(relativePath, "Đã tải ảnh đại diện bài viết.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Khong the luu anh dai dien bai viet {FileName}.", file.FileName);
            DeletePhysicalFile(physicalPath);
            return OperationResult<string>.Failure("Không thể tải ảnh đại diện. Vui lòng thử lại.");
        }
    }

    public void DeleteUploadedFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || !relativePath.StartsWith("/uploads/articles/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fileName = Path.GetFileName(relativePath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "articles", fileName);
        DeletePhysicalFile(physicalPath);
    }

    private static void DeletePhysicalFile(string physicalPath)
    {
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }
    }
}
