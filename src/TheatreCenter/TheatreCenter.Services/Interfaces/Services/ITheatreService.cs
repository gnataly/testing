using TheatreCenter.Domain.Models;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface ITheatreService
    {
        Task<Theatre?> GetTheatreByIdAsync(int id, int? currentUserId = null);
        Task<List<Theatre>> GetAllTheatresAsync(TheatreFilter filter, int? currentUserId = null);
        Task<int> GetCountAsync(TheatreFilter filter, int? currentUserId = null);

        Task<Theatre> CreateTheatreAsync(Theatre theatre);
        Task<Theatre> UpdateTheatreAsync(Theatre theatre);
        Task<bool> DeleteTheatreAsync(int id);
    }
}