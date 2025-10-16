using Microsoft.EntityFrameworkCore;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Data.Repositories
{
    public class MusicalRepository : IMusicalRepository
    {
        private readonly AppDbContext _context;

        public MusicalRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Musical?> GetByIdAsync(int id)
        {
            return await _context.Set<Musical>().FindAsync(id);
        }

        public async Task<IEnumerable<Musical>> GetAllAsync()
        {
            return await _context.Set<Musical>().ToListAsync();
        }

        public async Task AddAsync(Musical musical)
        {
            _context.Set<Musical>().AddAsync(musical);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Musical musical)
        {
            _context.Set<Musical>().Update(musical);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Musical musical)
        {
            _context.Set<Musical>().Remove(musical);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Musical>> GetByTheatreIdAsync(int theatreId)
        {
            return await _context.Set<Musical>().Where(m => m.TheatreId == theatreId).ToListAsync();
        }

        public async Task<IEnumerable<Musical>> GetByAgeRestrictionAsync(AgeRestriction ageRestriction)
        {
            return await _context.Set<Musical>().Where(m => m.AgeRestriction == ageRestriction).ToListAsync();
        }
    }
}
