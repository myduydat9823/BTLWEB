using BTLWEB.Repositories.Interfaces;
using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;
using BTLWEB.ViewModels.Entry;
using BTLWEB.Models.Competition;

namespace BTLWEB.Services;

public class EntryService : IEntryService
{
  private readonly IEntryRepository _entryRepository;
  private readonly ICompetitionRepository _competitionRepository;
  private readonly IPhotoRepository _photoRepository;
  private readonly IWebHostEnvironment _webHostEnvironment;
  private readonly ILogger<EntryService> _logger;

  public EntryService(
      IEntryRepository entryRepository,
      ICompetitionRepository competitionRepository,
      IPhotoRepository photoRepository,
      IWebHostEnvironment webHostEnvironment,
      ILogger<EntryService> logger)
  {
    _entryRepository = entryRepository;
    _competitionRepository = competitionRepository;
    _photoRepository = photoRepository;
    _webHostEnvironment = webHostEnvironment;
    _logger = logger;
  }

  public async Task<List<EntryListViewModel>> GetEntriesByCompetitionAsync(int competitionId)
  {
    try
    {
      var entries = await _entryRepository.GetByCompetitionIdAsync(competitionId);
      return entries.Select(MapToListViewModel).ToList();
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Khong the lay danh sach bai du thi cho cuoc thi {CompetitionId}.", competitionId);
      return [];
    }
  }

  public async Task<List<EntryListViewModel>> GetMyEntriesAsync(int userId)
  {
    try
    {
      var entries = await _entryRepository.GetByUserIdAsync(userId);
      return entries.Select(MapToListViewModel).ToList();
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Khong the lay danh sach bai du thi cua nguoi dung {UserId}.", userId);
      return [];
    }
  }

  public async Task<ReviewEntryViewModel?> GetEntryForReviewAsync(int entryId)
  {
    try
    {
      var entry = await _entryRepository.GetByIdAsync(entryId);
      if (entry is null) return null;

      return new ReviewEntryViewModel
      {
        EntryId = entry.Id,
        CompetitionId = entry.CompetitionId,
        UserId = entry.UserId,
        Status = (EntryStatus)entry.Status,
        AverageScore = entry.AverageScore,
        Rank = entry.Rank,
        Note = entry.AdminNote ?? string.Empty
      };
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Khong the lay thong tin bai du thi {EntryId} de danh gia.", entryId);
      return null;
    }
  }

  public async Task<OperationResult> SubmitAsync(SubmitEntryViewModel model, int userId)
  {
    try
    {
      var competition = await _competitionRepository.GetByIdAsync(model.CompetitionId);
      if (competition is null)
      {
        return OperationResult.Failure("Không tìm thấy cuộc thi.");
      }

      if (competition.Status != (int)CompetitionStatus.OpenForSubmission)
      {
        return OperationResult.Failure("Cuộc thi không trong trạng thái mở nhận bài.");
      }

      if (competition.SubmissionStartDate > DateTime.Now || competition.SubmissionEndDate < DateTime.Now)
      {
        return OperationResult.Failure("Thời gian gửi bài không hợp lệ.");
      }

      // Check if user has already submitted entries for this competition
      var existingEntries = await _entryRepository.GetByUserIdAsync(userId);
      var hasExistingEntry = existingEntries.Any(e => e.CompetitionId == model.CompetitionId && !e.EntryGroupId.HasValue);
      if (hasExistingEntry)
      {
        return OperationResult.Failure("Bạn đã gửi bài dự thi cho cuộc thi này rồi.");
      }

      // Validate files
      if (model.PhotoFiles == null || !model.PhotoFiles.Any())
      {
        return OperationResult.Failure("Vui lòng chọn ít nhất một ảnh.");
      }

      // Validate file count
      if (model.PhotoFiles.Count > 10)
      {
        return OperationResult.Failure("Bạn chỉ có thể gửi tối đa 10 ảnh.");
      }

      // Save photo files
      var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "competitions");
      Directory.CreateDirectory(uploadsDir);

      // Create a single EntryGroupId for all photos in this submission
      var entryGroupId = Guid.NewGuid();

      var submittedCount = 0;
      foreach (var photoFile in model.PhotoFiles)
      {
        // Validate file size (10MB max per file)
        if (photoFile.Length > 10 * 1024 * 1024)
        {
          return OperationResult.Failure($"Ảnh {photoFile.FileName} vượt quá kích thước tối đa 10MB.");
        }

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var fileExtension = Path.GetExtension(photoFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
          return OperationResult.Failure($"Định dạng ảnh {fileExtension} không được hỗ trợ. Chỉ chấp nhận: JPG, PNG, GIF, WEBP.");
        }

        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
          await photoFile.CopyToAsync(stream);
        }

        var photo = new Photo
        {
          Title = model.PhotoTitle.Trim(),
          Description = model.PhotoDescription?.Trim(),
          ImagePath = $"/uploads/competitions/{fileName}",
          UserId = userId,
          FileSize = photoFile.Length,
          FileExtension = fileExtension,
          UploadedAt = DateTime.Now
        };

        await _photoRepository.AddAsync(photo);

        var entry = new CompetitionEntry
        {
          CompetitionId = model.CompetitionId,
          UserId = userId,
          PhotoId = photo.Id,
          EntryGroupId = entryGroupId,
          SubmittedAt = DateTime.Now,
          Status = (int)EntryStatus.Pending
        };

        await _entryRepository.AddAsync(entry);
        submittedCount++;
      }

      _logger.LogInformation("User {UserId} submitted {Count} entries to competition {CompetitionId}.",
          userId, submittedCount, model.CompetitionId);

      return OperationResult.Success($"Gửi bài dự thi thành công. Bạn đã gửi {submittedCount} ảnh.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Loi khi gui bai du thi.");
      return OperationResult.Failure("Đã xảy ra lỗi khi gửi bài dự thi.");
    }
  }

  public async Task<OperationResult> ReviewAsync(ReviewEntryViewModel model)
  {
    try
    {
      var entry = await _entryRepository.GetByIdAsync(model.EntryId);
      if (entry is null)
      {
        return OperationResult.Failure("Không tìm thấy bài dự thi.");
      }

      entry.Status = (int)model.Status;
      entry.AverageScore = model.AverageScore;
      entry.Rank = model.Rank;
      entry.AdminNote = model.Note?.Trim();

      await _entryRepository.UpdateAsync(entry);
      _logger.LogInformation("Reviewed entry {EntryId}. Status: {Status}, Score: {Score}",
          model.EntryId, model.Status, model.AverageScore);

      return OperationResult.Success("Đánh giá bài dự thi thành công.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Loi khi danh gia bai du thi {EntryId}.", model.EntryId);
      return OperationResult.Failure("Đã xảy ra lỗi khi đánh giá bài dự thi.");
    }
  }

  public async Task<OperationResult> DeleteAsync(int entryId)
  {
    try
    {
      var entry = await _entryRepository.GetByIdAsync(entryId);
      if (entry is null)
      {
        return OperationResult.Failure("Không tìm thấy bài dự thi.");
      }

      // Delete photo file if exists
      if (entry.Photo is not null && !string.IsNullOrEmpty(entry.Photo.ImagePath))
      {
        var filePath = Path.Combine(_webHostEnvironment.WebRootPath,
            entry.Photo.ImagePath.TrimStart('/'));
        if (File.Exists(filePath))
        {
          File.Delete(filePath);
        }
      }

      await _entryRepository.DeleteAsync(entry);
      _logger.LogInformation("Deleted entry {EntryId}", entryId);
      return OperationResult.Success("Xóa bài dự thi thành công.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Loi khi xoa bai du thi {EntryId}.", entryId);
      return OperationResult.Failure("Đã xảy ra lỗi khi xóa bài dự thi.");
    }
  }

  private static EntryListViewModel MapToListViewModel(CompetitionEntry e)
  {
    return new EntryListViewModel
    {
      Id = e.Id,
      CompetitionId = e.CompetitionId,
      UserId = e.UserId,
      PhotoId = e.PhotoId,
      EntryGroupId = e.EntryGroupId,
      SubmittedAt = e.SubmittedAt,
      Status = (EntryStatus)e.Status,
      AverageScore = e.AverageScore,
      Rank = e.Rank,
      AdminNote = e.AdminNote ?? string.Empty,
      PhotoTitle = e.Photo?.Title ?? string.Empty,
      PhotoDescription = e.Photo?.Description ?? string.Empty,
      PhotoImagePath = e.Photo?.ImagePath ?? string.Empty,
      UserFullName = e.User?.FullName ?? string.Empty,
      UserName = e.User?.Username ?? string.Empty
    };
  }
}