using BTLWEB.Data;
using BTLWEB.Models.Competition;
using BTLWEB.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Repositories;

public class CompetitionRepository : ICompetitionRepository
{
  private readonly AppDbContext _dbContext;

  public CompetitionRepository(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public Task<List<Competition>> GetAllAsync()
  {
    return _dbContext.Competitions
        .AsNoTracking()
        .Include(x => x.Entries)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();
  }

  public Task<List<Competition>> GetActiveAsync()
  {
    return _dbContext.Competitions
        .AsNoTracking()
        .Include(x => x.Entries)
        .Where(x => x.Status == (int)CompetitionStatus.OpenForSubmission
                 || x.Status == (int)CompetitionStatus.Judging)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();
  }

  public Task<List<Competition>> GetByStatusAsync(int status)
  {
    return _dbContext.Competitions
        .AsNoTracking()
        .Include(x => x.Entries)
        .Where(x => x.Status == status)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();
  }

  public Task<Competition?> GetByIdAsync(int id)
  {
    return _dbContext.Competitions
        .AsNoTracking()
        .Include(x => x.Entries)
          .ThenInclude(e => e.Photo)
        .Include(x => x.CreatedByUser)
        .FirstOrDefaultAsync(x => x.Id == id);
  }

  public async Task AddAsync(Competition competition)
  {
    await _dbContext.Competitions.AddAsync(competition);
    await _dbContext.SaveChangesAsync();
  }

  public async Task UpdateAsync(Competition competition)
  {
    _dbContext.Competitions.Update(competition);
    await _dbContext.SaveChangesAsync();
  }

  public async Task DeleteAsync(Competition competition)
  {
    _dbContext.Competitions.Remove(competition);
    await _dbContext.SaveChangesAsync();
  }

  public Task<bool> ExistsAsync(int id)
  {
    return _dbContext.Competitions.AnyAsync(x => x.Id == id);
  }

  public Task<bool> HasEntriesAsync(int competitionId)
  {
    return _dbContext.CompetitionEntries.AnyAsync(x => x.CompetitionId == competitionId);
  }

  public Task<int> GetEntryCountAsync(int competitionId)
  {
    return _dbContext.CompetitionEntries.CountAsync(x => x.CompetitionId == competitionId);
  }
}