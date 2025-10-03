using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheatreCenter.Domain.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(int id);
        Task<IEnumerable<Role>> GetAllAsync();
        Task AddAsync(Role role);
        Task UpdateAsync(Role role);
        Task RemoveAsync(Role role);
        Task SaveChangesAsync();
        Task<IEnumerable<Role>> GetByMusicalIdAsync(int musicalId);
        Task<IEnumerable<Role>> GetByRoleTypeAsync(RoleType roleType);
    }
}