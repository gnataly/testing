using Microsoft.EntityFrameworkCore;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Data.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _context.Roles
                .Include(r => r.Musical)
                .Include(r => r.ActorRoles)
                    .ThenInclude(ar => ar.Actor)
                .Include(r => r.CastMembers)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Role>> GetAllAsync(RoleFilter filter)
        {
            var query = _context.Roles
                .Include(r => r.Musical)
                .Include(r => r.ActorRoles)
                    .ThenInclude(ar => ar.Actor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(r => r.Name.Contains(filter.Search));
            }

            if (filter.RoleType.HasValue)
            {
                query = query.Where(r => r.RoleType == filter.RoleType.Value);
            }

            if (filter.MusicalId.HasValue)
            {
                query = query.Where(r => r.MusicalId == filter.MusicalId.Value);
            }

            if (filter.ActorId.HasValue)
            {
                query = query.Where(r => r.ActorRoles.Any(ar => ar.ActorId == filter.ActorId.Value));
            }

            query = filter.Sort switch
            {
                "name_asc" => query.OrderBy(r => r.Name),
                "name_desc" => query.OrderByDescending(r => r.Name),
                "roleType_asc" => query.OrderBy(r => r.RoleType),
                "roleType_desc" => query.OrderByDescending(r => r.RoleType),
                "id_desc" => query.OrderByDescending(r => r.Id),
                _ => query.OrderBy(r => r.Id)
            };

            return await query.ToListAsync();
        }

        public async Task AddAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
        }

        public async Task UpdateAsync(Role role)
        {
            _context.Roles.Update(role);
        }

        public async Task RemoveAsync(Role role)
        {
            _context.Roles.Remove(role);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Role>> GetByMusicalIdAsync(int musicalId)
        {
            return await _context.Roles
                .Where(r => r.MusicalId == musicalId)
                .Include(r => r.Musical)
                .Include(r => r.ActorRoles)
                    .ThenInclude(ar => ar.Actor)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetByRoleTypeAsync(RoleType roleType)
        {
            return await _context.Roles
                .Where(r => r.RoleType == roleType)
                .Include(r => r.Musical)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetActorRolesAsync(int actorId)
        {
            return await _context.Roles
                .Where(r => r.ActorRoles.Any(ar => ar.ActorId == actorId))
                .Include(r => r.Musical)
                .ToListAsync();
        }
    }
}