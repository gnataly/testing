using Microsoft.EntityFrameworkCore;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheatreCenter.Data.Repositories
{
    public class ThemeRepository : IThemeRepository
    {
        private readonly AppDbContext _context;

        public ThemeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Theme?> GetByIdAsync(int id)
        {
            return await _context.Themes.FindAsync(id);
        }

        public async Task<IEnumerable<Theme>> GetAllAsync()
        {
            return await _context.Themes.ToListAsync();
        }

        public async Task AddAsync(Theme theme)
        {
            _context.Themes.AddAsync(theme);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Theme theme)
        {
            _context.Themes.Update(theme);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Theme theme)
        {
            _context.Themes.Remove(theme);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}