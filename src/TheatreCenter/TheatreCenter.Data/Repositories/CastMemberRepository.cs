using Microsoft.EntityFrameworkCore;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheatreCenter.Data.Repositories
{
    public class CastMemberRepository : ICastMemberRepository
    {
        private readonly AppDbContext _context;

        public CastMemberRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CastMember?> GetByIdAsync(int id)
        {
            return await _context.CastMembers.FindAsync(id);
        }

        public async Task<IEnumerable<CastMember>> GetAllAsync()
        {
            return await _context.CastMembers.ToListAsync();
        }

        public async Task AddAsync(CastMember castMember)
        {
            await _context.CastMembers.AddAsync(castMember);
        }

        public async Task UpdateAsync(CastMember castMember)
        {
            _context.CastMembers.Update(castMember);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(CastMember castMember)
        {
            _context.CastMembers.Remove(castMember);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CastMember>> GetByShowIdAsync(int showId)
        {
            return await _context.CastMembers
                .Where(cm => cm.ShowId == showId)
                .ToListAsync();
        }

        public async Task<IEnumerable<CastMember>> GetByActorIdAsync(int actorId)
        {
            return await _context.CastMembers
                .Where(cm => cm.ActorId == actorId)
                .ToListAsync();
        }

        public async Task<CastMember?> GetByShowAndRoleAsync(int showId, int roleId)
        {
            return await _context.CastMembers
                .FirstOrDefaultAsync(cm => cm.ShowId == showId && cm.RoleId == roleId);
        }
    }
}