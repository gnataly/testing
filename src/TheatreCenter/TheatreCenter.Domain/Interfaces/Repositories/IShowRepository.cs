using TheatreCenter.Domain.Models;

namespace TheatreCenter.Domain.Interfaces.Repositories;

public interface IShowRepository
{
    Task<Show?> GetByIdAsync(int id);
    Task<IEnumerable<Show>> GetAllAsync(ShowFilter filter);
    Task AddAsync(Show show);
    Task UpdateAsync(Show show);
    Task RemoveAsync(Show show);
    Task SaveChangesAsync();
    Task<IEnumerable<Show>> GetByMusicalIdAsync(int musicalId);
    Task<IEnumerable<Show>> GetUpcomingShowsAsync();
}