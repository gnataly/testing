using TheatreCenter.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface IThemeService
    {
        Task<Theme> GetByIdAsync(int id);
        Task<IEnumerable<Theme>> GetAllAsync();
        Task<Theme> CreateAsync(Theme theme);
        Task<Theme> UpdateAsync(Theme theme);
        Task<bool> DeleteAsync(int id);
    }
}