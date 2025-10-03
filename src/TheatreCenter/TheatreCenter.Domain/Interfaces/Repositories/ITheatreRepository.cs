using TheatreCenter.Domain.Models;

namespace TheatreCenter.Domain.Interfaces.Repositories
{
    public interface ITheatreRepository
    {
        Task<Theatre?> GetByIdAsync(int id);
        Task<IEnumerable<Theatre>> GetAllAsync();
        Task AddAsync(Theatre theatre);
        Task UpdateAsync(Theatre theatre);
        Task RemoveAsync(Theatre theatre);
        Task SaveChangesAsync();
    }
}
