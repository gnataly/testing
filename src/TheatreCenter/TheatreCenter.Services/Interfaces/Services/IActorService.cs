using TheatreCenter.DTOs.Actor;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;


namespace TheatreCenter.Services.Interfaces.Services
{
    public interface IActorService
    {
        Task<Actor> GetActorByIdAsync(int id);
        Task<IEnumerable<Actor>> GetAllActorsAsync();
        Task<Actor> CreateActorAsync(Actor actor);
        Task<Actor> UpdateActorAsync(Actor actor);
        Task<bool> DeleteActorAsync(int id);
        Task<IEnumerable<Actor>> GetActorsByVoiceTypeAsync(VoiceType voiceType);
    }
}
