using TheatreCenter.Domain.Models;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface IShowService
    {
        Task<Show> GetByIdAsync(int id);
        Task<IEnumerable<Show>> GetAllAsync(ShowFilter filter);
        Task<Show> CreateAsync(Show show);
        Task<Show> UpdateAsync(Show show);
        Task<bool> DeleteAsync(int id);
    }
}
