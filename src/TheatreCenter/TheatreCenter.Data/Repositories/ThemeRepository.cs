using Microsoft.EntityFrameworkCore;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;

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
            return await _context.Themes
                .Include(t => t.MusicalThemes)
                    .ThenInclude(mt => mt.Musical)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Theme>> GetAllAsync(ThemeFilter filter)
        {
            var query = _context.Themes
                .Include(t => t.MusicalThemes)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(t => t.Name.Contains(filter.Search));
            }

            query = filter.Sort switch
            {
                "name_asc" => query.OrderBy(t => t.Name),
                "name_desc" => query.OrderByDescending(t => t.Name),
                "id_desc" => query.OrderByDescending(t => t.Id),
                _ => query.OrderBy(t => t.Id)
            };

            return await query.ToListAsync();
        }

        public async Task AddAsync(Theme theme)
        {
            await _context.Themes.AddAsync(theme);
        }

        public async Task UpdateAsync(Theme theme)
        {
            _context.Themes.Update(theme);
        }

        public async Task RemoveAsync(Theme theme)
        {
            _context.Themes.Remove(theme);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
