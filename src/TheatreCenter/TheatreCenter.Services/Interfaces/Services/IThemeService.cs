using TheatreCenter.Domain.Models;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface IThemeService
    {
        Task<Theme> GetByIdAsync(int id);
        Task<IEnumerable<Theme>> GetAllAsync(ThemeFilter filter);
        Task<Theme> CreateAsync(Theme theme);
        Task<Theme> UpdateAsync(Theme theme);
        Task<bool> DeleteAsync(int id);
    }
}
