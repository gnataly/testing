using TheatreCenter.DTOs.Theatre;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface ITheatreService
    {
        Task<Theatre?> GetTheatreByIdAsync(int id);
        Task<IEnumerable<Theatre>> GetAllTheatresAsync();
        Task<Theatre> CreateTheatreAsync(Theatre theatre);
        Task<Theatre> UpdateTheatreAsync(Theatre theatre);
        Task<bool> DeleteTheatreAsync(int id);
    }
}
