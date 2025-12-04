using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Domain.Interfaces.Repositories;

public interface IActorRepository
{
    Task<Actor?> GetByIdAsync(int id);
    Task<List<Actor>> GetAllAsync(ActorFilter filter);
    Task<int> GetCountAsync(ActorFilter filter);
    Task AddAsync(Actor actor);
    Task UpdateAsync(Actor actor);
    Task RemoveAsync(Actor actor);
    Task SaveChangesAsync();
    Task<bool> AddActorToRoleAsync(int actorId, int roleId);
    Task<bool> RemoveActorFromRoleAsync(int actorId, int roleId);
}