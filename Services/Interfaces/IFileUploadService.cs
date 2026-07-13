using BTLWEB.ViewModels;

namespace BTLWEB.Services.Interfaces;

public interface IFileUploadService
{
    Task<OperationResult<string>> UploadArticleThumbnailAsync(IFormFile? file, CancellationToken cancellationToken = default);
    void DeleteUploadedFile(string? relativePath);
}
