using BTLWEB.ViewModels;
using BTLWEB.ViewModels.Competition;
using BTLWEB.ViewModels.Entry;

namespace BTLWEB.Services.Interfaces;

public interface ICompetitionService
{
  Task<List<CompetitionListViewModel>> GetAllAsync();
  Task<List<CompetitionListViewModel>> GetActiveAsync();
  Task<List<CompetitionListViewModel>> GetByStatusAsync(int status);
  Task<CompetitionDetailViewModel?> GetByIdAsync(int id);
  Task<CreateCompetitionViewModel?> GetForEditAsync(int id);
  Task<OperationResult> CreateAsync(CreateCompetitionViewModel model, int createdByUserId);
  Task<OperationResult> UpdateAsync(CreateCompetitionViewModel model);
  Task<OperationResult> DeleteAsync(int id);
  Task<OperationResult> ChangeStatusAsync(int id, int newStatus);
  Task<List<EntryListViewModel>> GetEntriesByCompetitionAsync(int competitionId);
}
