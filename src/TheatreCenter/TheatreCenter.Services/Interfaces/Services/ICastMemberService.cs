using TheatreCenter.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface ICastMemberService
    {
        Task<CastMember> GetByIdAsync(int id);
        Task<IEnumerable<CastMember>> GetAllAsync();
        Task<CastMember> CreateAsync(CastMember castMember);
        Task<CastMember> UpdateAsync(CastMember castMember);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<CastMember>> GetByShowIdAsync(int showId);
        Task<IEnumerable<CastMember>> GetByActorIdAsync(int actorId);
    }
}