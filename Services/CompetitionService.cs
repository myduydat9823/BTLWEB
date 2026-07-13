using BTLWEB.Repositories.Interfaces;
using BTLWEB.Services.Interfaces;
using BTLWEB.ViewModels;
using BTLWEB.ViewModels.Competition;
using BTLWEB.Models.Competition;

namespace BTLWEB.Services;

public class CompetitionService : ICompetitionService
{
  private readonly ICompetitionRepository _competitionRepository;
  private readonly ILogger<CompetitionService> _logger;

  public CompetitionService(
      ICompetitionRepository competitionRepository,
      ILogger<CompetitionService> logger)
  {
    _competitionRepository = competitionRepository;
    _logger = logger;
  }

  public async Task<List<CompetitionListViewModel>> GetAllAsync()
  {
    try
    {
      var competitions = await _competitionRepository.GetAllAsync();
      return competitions.Select(MapToListViewModel).ToList();
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Khong the lay danh sach cuoc thi.");
      return [];
    }
  }

  public async Task<List<CompetitionListViewModel>> GetActiveAsync()
  {
    try
    {
      var competitions = await _competitionRepository.GetActiveAsync();
      return competitions.Select(MapToListViewModel).ToList();
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Khong the lay danh sach cuoc thi dang hoat dong.");
      return [];
    }
  }

  public async Task<CompetitionDetailViewModel?> GetByIdAsync(int id)
  {
    try
    {
      var competition = await _competitionRepository.GetByIdAsync(id);
      if (competition is null) return null;

      return new CompetitionDetailViewModel
      {
        Id = competition.Id,
        Name = competition.Name,
        Description = competition.Description ?? string.Empty,
        Rules = competition.Rules ?? string.Empty,
        SubmissionStartDate = competition.SubmissionStartDate,
        SubmissionEndDate = competition.SubmissionEndDate,
        Status = (CompetitionStatus)competition.Status,
        EntryCount = competition.Entries?.Count ?? 0,
        ImageUrl = competition.ImageUrl,
        Entries = competition.Entries?.Select(e => new ViewModels.Entry.EntryListViewModel
        {
          Id = e.Id,
          CompetitionId = e.CompetitionId,
          UserId = e.UserId,
          PhotoId = e.PhotoId,
          SubmittedAt = e.SubmittedAt,
          Status = (EntryStatus)e.Status,
          AverageScore = e.AverageScore,
          Rank = e.Rank,
          AdminNote = e.AdminNote ?? string.Empty,
          PhotoTitle = e.Photo?.Title ?? string.Empty,
          PhotoDescription = e.Photo?.Description ?? string.Empty,
          PhotoImagePath = e.Photo?.ImagePath ?? string.Empty
        }).ToList() ?? []
      };
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Khong the lay thong tin cuoc thi {CompetitionId}.", id);
      return null;
    }
  }

  public async Task<CreateCompetitionViewModel?> GetForEditAsync(int id)
  {
    try
    {
      var competition = await _competitionRepository.GetByIdAsync(id);
      if (competition is null) return null;

      return new CreateCompetitionViewModel
      {
        Id = competition.Id,
        Name = competition.Name,
        Description = competition.Description ?? string.Empty,
        Rules = competition.Rules ?? string.Empty,
        SubmissionStartDate = competition.SubmissionStartDate,
        SubmissionEndDate = competition.SubmissionEndDate,
        Status = competition.Status,
        ImageUrl = competition.ImageUrl
      };
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Khong the lay thong tin cuoc thi {CompetitionId} de chinh sua.", id);
      return null;
    }
  }

  public async Task<OperationResult> CreateAsync(CreateCompetitionViewModel model, int createdByUserId)
  {
    try
    {
      var competition = new BTLWEB.Models.Competition.Competition
      {
        Name = model.Name.Trim(),
        Description = model.Description?.Trim(),
        Rules = model.Rules?.Trim(),
        SubmissionStartDate = model.SubmissionStartDate,
        SubmissionEndDate = model.SubmissionEndDate,
        Status = model.Status,
        ImageUrl = model.ImageUrl,
        CreatedByUserId = createdByUserId,
        CreatedAt = DateTime.Now
      };

      await _competitionRepository.AddAsync(competition);
      _logger.LogInformation("Created competition {CompetitionId}: {Name}", competition.Id, competition.Name);
      return OperationResult.Success("Tạo cuộc thi thành công.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Loi khi tao cuoc thi.");
      return OperationResult.Failure("Đã xảy ra lỗi khi tạo cuộc thi.");
    }
  }

  public async Task<OperationResult> UpdateAsync(CreateCompetitionViewModel model)
  {
    try
    {
      var existing = await _competitionRepository.GetByIdAsync(model.Id);
      if (existing is null)
      {
        return OperationResult.Failure("Không tìm thấy cuộc thi.");
      }

      existing.Name = model.Name.Trim();
      existing.Description = model.Description?.Trim();
      existing.Rules = model.Rules?.Trim();
      existing.SubmissionStartDate = model.SubmissionStartDate;
      existing.SubmissionEndDate = model.SubmissionEndDate;
      existing.Status = model.Status;
      existing.ImageUrl = model.ImageUrl;
      existing.UpdatedAt = DateTime.Now;

      await _competitionRepository.UpdateAsync(existing);
      _logger.LogInformation("Updated competition {CompetitionId}", model.Id);
      return OperationResult.Success("Cập nhật cuộc thi thành công.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Loi khi cap nhat cuoc thi {CompetitionId}.", model.Id);
      return OperationResult.Failure("Đã xảy ra lỗi khi cập nhật cuộc thi.");
    }
  }

  public async Task<OperationResult> DeleteAsync(int id)
  {
    try
    {
      var competition = await _competitionRepository.GetByIdAsync(id);
      if (competition is null)
      {
        return OperationResult.Failure("Không tìm thấy cuộc thi.");
      }

      if (await _competitionRepository.HasEntriesAsync(id))
      {
        return OperationResult.Failure("Không thể xóa cuộc thi đã có bài dự thi.");
      }

      await _competitionRepository.DeleteAsync(competition);
      _logger.LogInformation("Deleted competition {CompetitionId}", id);
      return OperationResult.Success("Xóa cuộc thi thành công.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Loi khi xoa cuoc thi {CompetitionId}.", id);
      return OperationResult.Failure("Đã xảy ra lỗi khi xóa cuộc thi.");
    }
  }

  public async Task<OperationResult> ChangeStatusAsync(int id, int newStatus)
  {
    try
    {
      var competition = await _competitionRepository.GetByIdAsync(id);
      if (competition is null)
      {
        return OperationResult.Failure("Không tìm thấy cuộc thi.");
      }

      competition.Status = newStatus;
      competition.UpdatedAt = DateTime.Now;

      await _competitionRepository.UpdateAsync(competition);
      _logger.LogInformation("Changed competition {CompetitionId} status to {Status}", id, newStatus);
      return OperationResult.Success("Cập nhật trạng thái cuộc thi thành công.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Loi khi cap nhat trang thai cuoc thi {CompetitionId}.", id);
      return OperationResult.Failure("Đã xảy ra lỗi khi cập nhật trạng thái.");
    }
  }

  private static CompetitionListViewModel MapToListViewModel(BTLWEB.Models.Competition.Competition c)
  {
    return new CompetitionListViewModel
    {
      Id = c.Id,
      Name = c.Name,
      SubmissionStartDate = c.SubmissionStartDate,
      SubmissionEndDate = c.SubmissionEndDate,
      EntryCount = c.Entries?.Count ?? 0,
      Status = (CompetitionStatus)c.Status,
      ImageUrl = c.ImageUrl
    };
  }
}