using Microsoft.EntityFrameworkCore;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;

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
            return await _context.CastMembers
                .Include(cm => cm.Show)
                    .ThenInclude(s => s.Musical)
                .Include(cm => cm.Role)
                .Include(cm => cm.Actor)
                .FirstOrDefaultAsync(cm => cm.Id == id);
        }

        public async Task<IEnumerable<CastMember>> GetAllAsync(CastMemberFilter filter)
        {
            var query = _context.CastMembers
                .Include(cm => cm.Show)
                    .ThenInclude(s => s.Musical)
                .Include(cm => cm.Role)
                .Include(cm => cm.Actor)
                .AsQueryable();

            if (filter.ShowId.HasValue)
            {
                query = query.Where(cm => cm.ShowId == filter.ShowId.Value);
            }

            if (filter.RoleId.HasValue)
            {
                query = query.Where(cm => cm.RoleId == filter.RoleId.Value);
            }

            if (filter.ActorId.HasValue)
            {
                query = query.Where(cm => cm.ActorId == filter.ActorId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task AddAsync(CastMember castMember)
        {
            await _context.CastMembers.AddAsync(castMember);
        }

        public async Task UpdateAsync(CastMember castMember)
        {
            _context.CastMembers.Update(castMember);
        }

        public async Task RemoveAsync(CastMember castMember)
        {
            _context.CastMembers.Remove(castMember);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CastMember>> GetByShowIdAsync(int showId)
        {
            return await _context.CastMembers
                .Where(cm => cm.ShowId == showId)
                .Include(cm => cm.Show)
                .Include(cm => cm.Role)
                .Include(cm => cm.Actor)
                .ToListAsync();
        }

        public async Task<IEnumerable<CastMember>> GetByActorIdAsync(int actorId)
        {
            return await _context.CastMembers
                .Where(cm => cm.ActorId == actorId)
                .Include(cm => cm.Show)
                    .ThenInclude(s => s.Musical)
                .Include(cm => cm.Role)
                .ToListAsync();
        }

        public async Task<CastMember?> GetByShowAndRoleAsync(int showId, int roleId)
        {
            return await _context.CastMembers
                .FirstOrDefaultAsync(cm => cm.ShowId == showId && cm.RoleId == roleId);
        }
    }
}
