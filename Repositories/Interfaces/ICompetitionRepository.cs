using BTLWEB.Models.Competition;

namespace BTLWEB.Repositories.Interfaces;

public interface ICompetitionRepository
{
  Task<List<Competition>> GetAllAsync();
  Task<List<Competition>> GetActiveAsync();
  Task<List<Competition>> GetByStatusAsync(int status);
  Task<Competition?> GetByIdAsync(int id);
  Task AddAsync(Competition competition);
  Task UpdateAsync(Competition competition);
  Task DeleteAsync(Competition competition);
  Task<bool> ExistsAsync(int id);
  Task<bool> HasEntriesAsync(int competitionId);
  Task<int> GetEntryCountAsync(int competitionId);
}