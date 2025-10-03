using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Domain.Interfaces.Repositories
{
    public interface IActorRepository
    {
        Task<Actor?> GetByIdAsync(int id);
        Task<IEnumerable<Actor>> GetAllAsync();
        Task AddAsync(Actor actor);
        Task UpdateAsync(Actor actor);
        Task RemoveAsync(Actor actor);
        Task SaveChangesAsync();
        Task<IEnumerable<Actor>> GetByVoiceTypeAsync(VoiceType voiceType);
    }
}
