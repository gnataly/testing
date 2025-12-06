using TheatreCenter.Domain.Models;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface ICastMemberService
    {
        Task<CastMember> GetByIdAsync(int id);
        Task<IEnumerable<CastMember>> GetAllAsync(CastMemberFilter filter);
        Task<CastMember> CreateAsync(CastMember castMember);
        Task<CastMember> UpdateAsync(CastMember castMember);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<CastMember>> GetByShowIdAsync(int showId);
        Task<IEnumerable<CastMember>> GetByActorIdAsync(int actorId);
    }
}