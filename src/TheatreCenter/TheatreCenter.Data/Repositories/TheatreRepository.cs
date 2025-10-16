using Microsoft.EntityFrameworkCore;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Data.Repositories
{
    public class TheatreRepository : ITheatreRepository
    {
        private readonly AppDbContext _context;

        public TheatreRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Theatre?> GetByIdAsync(int id)
        {
            return await _context.Set<Theatre>().FindAsync(id);
        }

        public async Task<IEnumerable<Theatre>> GetAllAsync()
        {
            return await _context.Set<Theatre>().ToListAsync();
        }

        public async Task AddAsync(Theatre theatre)
        {
            _context.Set<Theatre>().AddAsync(theatre);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Theatre theatre)
        {
            _context.Set<Theatre>().Update(theatre);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Theatre theatre)
        {
            _context.Set<Theatre>().Remove(theatre);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
