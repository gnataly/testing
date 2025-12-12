using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Domain.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int id);
    Task<IEnumerable<Role>> GetAllAsync(RoleFilter filter);
    Task AddAsync(Role role);
    Task UpdateAsync(Role role);
    Task RemoveAsync(Role role);
    Task SaveChangesAsync();
    Task<IEnumerable<Role>> GetByMusicalIdAsync(int musicalId);
    Task<IEnumerable<Role>> GetByRoleTypeAsync(RoleType roleType);
    Task<IEnumerable<Role>> GetActorRolesAsync(int actorId);
}
