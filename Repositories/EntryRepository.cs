using BTLWEB.Data;
using BTLWEB.Models.Competition;
using BTLWEB.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Repositories;

public class EntryRepository : IEntryRepository
{
  private readonly AppDbContext _dbContext;

  public EntryRepository(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public Task<List<CompetitionEntry>> GetAllAsync()
  {
    return _dbContext.CompetitionEntries
        .AsNoTracking()
        .Include(x => x.Competition)
        .Include(x => x.User)
        .Include(x => x.Photo)
        .OrderByDescending(x => x.SubmittedAt)
        .ToListAsync();
  }

  public Task<List<CompetitionEntry>> GetByCompetitionIdAsync(int competitionId)
  {
    return _dbContext.CompetitionEntries
        .AsNoTracking()
        .Include(x => x.User)
        .Include(x => x.Photo)
        .Where(x => x.CompetitionId == competitionId)
        .OrderByDescending(x => x.SubmittedAt)
        .ToListAsync();
  }

  public Task<List<CompetitionEntry>> GetByUserIdAsync(int userId)
  {
    return _dbContext.CompetitionEntries
        .AsNoTracking()
        .Include(x => x.Competition)
        .Include(x => x.Photo)
        .Where(x => x.UserId == userId)
        .OrderByDescending(x => x.SubmittedAt)
        .ToListAsync();
  }

  public Task<CompetitionEntry?> GetByIdAsync(int id)
  {
    return _dbContext.CompetitionEntries
        .AsNoTracking()
        .Include(x => x.Competition)
        .Include(x => x.User)
        .Include(x => x.Photo)
        .FirstOrDefaultAsync(x => x.Id == id);
  }

  public async Task AddAsync(CompetitionEntry entry)
  {
    await _dbContext.CompetitionEntries.AddAsync(entry);
    await _dbContext.SaveChangesAsync();
  }

  public async Task UpdateAsync(CompetitionEntry entry)
  {
    _dbContext.CompetitionEntries.Update(entry);
    await _dbContext.SaveChangesAsync();
  }

  public async Task DeleteAsync(CompetitionEntry entry)
  {
    _dbContext.CompetitionEntries.Remove(entry);
    await _dbContext.SaveChangesAsync();
  }

  public Task<bool> ExistsAsync(int id)
  {
    return _dbContext.CompetitionEntries.AnyAsync(x => x.Id == id);
  }

  public Task<bool> UserHasEntryInCompetitionAsync(int userId, int competitionId)
  {
    return _dbContext.CompetitionEntries.AnyAsync(x => x.UserId == userId && x.CompetitionId == competitionId);
  }

  public Task<int> GetEntryCountByCompetitionAsync(int competitionId)
  {
    return _dbContext.CompetitionEntries.CountAsync(x => x.CompetitionId == competitionId);
  }
}