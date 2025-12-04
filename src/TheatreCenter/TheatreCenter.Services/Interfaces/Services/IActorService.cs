using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface IActorService
    {
        Task<Actor> GetActorByIdAsync(int id, int? currentUserId = null);
        Task<List<Actor>> GetAllActorsAsync(ActorFilter filter, int? currentUserId = null);
        Task<int> GetCountAsync(ActorFilter filter, int? currentUserId = null);
        Task<Actor> CreateActorAsync(Actor actor);
        Task<Actor> UpdateActorAsync(Actor actor);
        Task<bool> DeleteActorAsync(int id);
        Task<bool> AddActorToRoleAsync(int actorId, int roleId);
        Task<bool> RemoveActorFromRoleAsync(int actorId, int roleId);
    }
}