using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface IMusicalService
    {
        Task<Musical> GetMusicalByIdAsync(int id, int? currentUserId = null);
        Task<List<Musical>> GetAllMusicalsAsync(MusicalFilter filter, int? currentUserId = null);
        Task<int> GetCountAsync(MusicalFilter filter, int? currentUserId = null);
        Task<Musical> CreateMusicalAsync(Musical musical);
        Task<Musical> UpdateMusicalAsync(Musical musical);
        Task<bool> DeleteMusicalAsync(int id);
        Task<bool> AddThemeToMusicalAsync(int musicalId, int themeId);
        Task<bool> RemoveThemeFromMusicalAsync(int musicalId, int themeId);
    }
}
