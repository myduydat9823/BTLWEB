using BTLWEB.Models.Competition;

namespace BTLWEB.Repositories.Interfaces;

public interface IEntryRepository
{
  Task<List<CompetitionEntry>> GetAllAsync();
  Task<List<CompetitionEntry>> GetByCompetitionIdAsync(int competitionId);
  Task<List<CompetitionEntry>> GetByUserIdAsync(int userId);
  Task<CompetitionEntry?> GetByIdAsync(int id);
  Task AddAsync(CompetitionEntry entry);
  Task UpdateAsync(CompetitionEntry entry);
  Task DeleteAsync(CompetitionEntry entry);
  Task<bool> ExistsAsync(int id);
  Task<bool> UserHasEntryInCompetitionAsync(int userId, int competitionId);
  Task<int> GetEntryCountByCompetitionAsync(int competitionId);
}