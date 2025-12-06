using TheatreCenter.Domain.Models;

namespace TheatreCenter.Domain.Interfaces.Repositories
{
    public interface ITheatreRepository
    {
        Task<Theatre?> GetByIdAsync(int id);
        Task<List<Theatre>> GetAllAsync(TheatreFilter filter);
        Task<int> GetCountAsync(TheatreFilter filter);
        Task AddAsync(Theatre theatre);
        Task UpdateAsync(Theatre theatre);
        Task RemoveAsync(Theatre theatre);
        Task SaveChangesAsync();
    }
}