using TheatreCenter.Domain.Models;

namespace TheatreCenter.Domain.Interfaces.Repositories;

public interface IThemeRepository
{
    Task<Theme?> GetByIdAsync(int id);
    Task<IEnumerable<Theme>> GetAllAsync(ThemeFilter filter);
    Task AddAsync(Theme theme);
    Task UpdateAsync(Theme theme);
    Task RemoveAsync(Theme theme);
    Task SaveChangesAsync();
}