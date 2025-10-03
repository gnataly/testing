using TheatreCenter.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheatreCenter.Domain.Interfaces.Repositories
{
    public interface ICastMemberRepository
    {
        Task<CastMember?> GetByIdAsync(int id);
        Task<IEnumerable<CastMember>> GetAllAsync();
        Task AddAsync(CastMember castMember);
        Task UpdateAsync(CastMember castMember);
        Task RemoveAsync(CastMember castMember);
        Task SaveChangesAsync();
        Task<IEnumerable<CastMember>> GetByShowIdAsync(int showId);
        Task<IEnumerable<CastMember>> GetByActorIdAsync(int actorId);
        Task<CastMember?> GetByShowAndRoleAsync(int showId, int roleId);
    }
}