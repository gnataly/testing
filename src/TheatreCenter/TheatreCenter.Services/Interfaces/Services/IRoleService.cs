using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface IRoleService
    {
        Task<Role> GetByIdAsync(int id);
        Task<IEnumerable<Role>> GetAllAsync(RoleFilter filter);
        Task<Role> CreateAsync(Role role);
        Task<Role> UpdateAsync(Role role);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Role>> GetByMusicalIdAsync(int musicalId);
        Task<IEnumerable<Role>> GetByRoleTypeAsync(RoleType roleType);
        Task<IEnumerable<Role>> GetActorRolesAsync(int actorId);
    }
}