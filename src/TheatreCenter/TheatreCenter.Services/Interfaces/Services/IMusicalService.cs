using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface IMusicalService
    {
        Task<Musical> GetMusicalByIdAsync(int id);
        Task<IEnumerable<Musical>> GetAllMusicalsAsync();
        Task<Musical> CreateMusicalAsync(Musical musical);
        Task<Musical> UpdateMusicalAsync(Musical musical);
        Task<bool> DeleteMusicalAsync(int id);
        Task<IEnumerable<Musical>> GetMusicalsByTheatreAsync(int theatreId);
        Task<IEnumerable<Musical>> GetMusicalsByAgeRestrictionAsync(AgeRestriction ageRestriction);
    }
}
