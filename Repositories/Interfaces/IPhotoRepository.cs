using BTLWEB.Models.Competition;

namespace BTLWEB.Repositories.Interfaces;

public interface IPhotoRepository
{
  Task<Photo?> GetByIdAsync(int id);
  Task AddAsync(Photo photo);
  Task UpdateAsync(Photo photo);
  Task DeleteAsync(Photo photo);
}