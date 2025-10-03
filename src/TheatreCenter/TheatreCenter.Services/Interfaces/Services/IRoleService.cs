using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface IRoleService
    {
        Task<Role> GetByIdAsync(int id);
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role> CreateAsync(Role role);
        Task<Role> UpdateAsync(Role role);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Role>> GetByMusicalIdAsync(int musicalId);
        Task<IEnumerable<Role>> GetByRoleTypeAsync(RoleType roleType);
    }
}