using BTLWEB.ViewModels;
using BTLWEB.ViewModels.Entry;

namespace BTLWEB.Services.Interfaces;

public interface IEntryService
{
  Task<List<EntryListViewModel>> GetEntriesByCompetitionAsync(int competitionId);
  Task<List<EntryListViewModel>> GetMyEntriesAsync(int userId);
  Task<ReviewEntryViewModel?> GetEntryForReviewAsync(int entryId);
  Task<OperationResult> SubmitAsync(SubmitEntryViewModel model, int userId);
  Task<OperationResult> ReviewAsync(ReviewEntryViewModel model);
  Task<OperationResult> DeleteAsync(int entryId);
}