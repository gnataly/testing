using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Domain.Interfaces.Repositories;

public interface IMusicalRepository
{
    Task<Musical?> GetByIdAsync(int id);
    Task<List<Musical>> GetAllAsync(MusicalFilter filter);
    Task<int> GetCountAsync(MusicalFilter filter);
    Task AddAsync(Musical musical);
    Task UpdateAsync(Musical musical);
    Task RemoveAsync(Musical musical);
    Task SaveChangesAsync();
    Task<bool> AddThemeToMusicalAsync(int musicalId, int themeId);
    Task<bool> RemoveThemeFromMusicalAsync(int musicalId, int themeId);
}
