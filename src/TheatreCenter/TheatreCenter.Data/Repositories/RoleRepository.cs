using Microsoft.EntityFrameworkCore;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            return await _context.Roles.FindAsync(id);
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task AddAsync(Role role)
        {
            _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Role role)
        {
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Role role)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Role>> GetByMusicalIdAsync(int musicalId)
        {
            return await _context.Roles
                .Where(r => r.MusicalId == musicalId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetByRoleTypeAsync(RoleType roleType)
        {
            return await _context.Roles
                .Where(r => r.RoleType == roleType)
                .ToListAsync();
        }
    }
}