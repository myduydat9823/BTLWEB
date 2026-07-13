using BTLWEB.Data;
using BTLWEB.Models.Competition;
using BTLWEB.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Repositories;

public class PhotoRepository : IPhotoRepository
{
  private readonly AppDbContext _dbContext;

  public PhotoRepository(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public Task<Photo?> GetByIdAsync(int id)
  {
    return _dbContext.Photos
        .AsNoTracking()
        .Include(x => x.User)
        .FirstOrDefaultAsync(x => x.Id == id);
  }

  public async Task AddAsync(Photo photo)
  {
    await _dbContext.Photos.AddAsync(photo);
    await _dbContext.SaveChangesAsync();
  }

  public async Task UpdateAsync(Photo photo)
  {
    _dbContext.Photos.Update(photo);
    await _dbContext.SaveChangesAsync();
  }

  public async Task DeleteAsync(Photo photo)
  {
    _dbContext.Photos.Remove(photo);
    await _dbContext.SaveChangesAsync();
  }
}