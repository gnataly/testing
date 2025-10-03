using TheatreCenter.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheatreCenter.Domain.Interfaces.Repositories
{
    public interface IThemeRepository
    {
        Task<Theme?> GetByIdAsync(int id);
        Task<IEnumerable<Theme>> GetAllAsync();
        Task AddAsync(Theme theme);
        Task UpdateAsync(Theme theme);
        Task RemoveAsync(Theme theme);
        Task SaveChangesAsync();
    }
}