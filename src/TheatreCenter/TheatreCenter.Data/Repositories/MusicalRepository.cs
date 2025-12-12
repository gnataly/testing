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
            return await _context.Musicals
                .Include(m => m.Theatre)
                .Include(m => m.Shows)
                .Include(m => m.Roles)
                .Include(m => m.MusicalThemes)
                    .ThenInclude(mt => mt.Theme)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Musical>> GetAllAsync(MusicalFilter filter)
        {
            var query = ApplySorting(ApplyFilters(BuildBaseQuery(), filter), filter.Sort);

            return await ApplyPaging(query, filter.Page, filter.PageSize).ToListAsync();
        }

        public async Task<int> GetCountAsync(MusicalFilter filter)
        {
            var query = ApplySorting(ApplyFilters(BuildBaseQuery(), filter), filter.Sort);

            return await query.CountAsync();
        }

        public async Task AddAsync(Musical musical)
        {
            await _context.Musicals.AddAsync(musical);
        }

        public async Task UpdateAsync(Musical musical)
        {
            _context.Musicals.Update(musical);
        }

        public async Task RemoveAsync(Musical musical)
        {
            _context.Musicals.Remove(musical);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Musical>> GetByTheatreIdAsync(int theatreId)
        {
            return await _context.Musicals
                .Where(m => m.TheatreId == theatreId)
                .Include(m => m.Theatre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Musical>> GetByAgeRestrictionAsync(AgeRestriction ageRestriction)
        {
            return await _context.Musicals
                .Where(m => m.AgeRestriction == ageRestriction)
                .Include(m => m.Theatre)
                .ToListAsync();
        }

        public async Task<bool> AddThemeToMusicalAsync(int musicalId, int themeId)
        {
            var existing = await _context.MusicalThemes
                .FirstOrDefaultAsync(mt => mt.MusicalId == musicalId && mt.ThemeId == themeId);

            if (existing != null) return false;

            var musicalTheme = new MusicalTheme(musicalId, themeId);
            await _context.MusicalThemes.AddAsync(musicalTheme);
            return true;
        }

        public async Task<bool> RemoveThemeFromMusicalAsync(int musicalId, int themeId)
        {
            var musicalTheme = await _context.MusicalThemes
                .FirstOrDefaultAsync(mt => mt.MusicalId == musicalId && mt.ThemeId == themeId);

            if (musicalTheme == null) return false;

            _context.MusicalThemes.Remove(musicalTheme);
            return true;
        }

        public async Task<IEnumerable<Theme>> GetMusicalThemesAsync(int musicalId)
        {
            return await _context.Themes
                .Where(t => t.MusicalThemes.Any(mt => mt.MusicalId == musicalId))
                .ToListAsync();
        }

        private IQueryable<Musical> BuildBaseQuery()
        {
            return _context.Musicals
                .Include(m => m.Theatre)
                .Include(m => m.MusicalThemes)
                    .ThenInclude(mt => mt.Theme)
                .AsQueryable();
        }

        private static IQueryable<Musical> ApplyFilters(IQueryable<Musical> query, MusicalFilter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                query = query.Where(m => m.Title.Contains(filter.Search) ||
                                         m.Description.Contains(filter.Search));
            }

            if (filter.AgeRestriction.HasValue)
            {
                query = query.Where(m => m.AgeRestriction == filter.AgeRestriction.Value);
            }

            if (filter.TheatreId.HasValue)
            {
                query = query.Where(m => m.TheatreId == filter.TheatreId.Value);
            }

            if (filter.ThemeId.HasValue)
            {
                query = query.Where(m => m.MusicalThemes.Any(mt => mt.ThemeId == filter.ThemeId.Value));
            }

            return query;
        }

        private static IQueryable<Musical> ApplySorting(IQueryable<Musical> query, string? sort) =>
            sort switch
            {
                "title_asc" => query.OrderBy(m => m.Title),
                "title_desc" => query.OrderByDescending(m => m.Title),
                "duration_asc" => query.OrderBy(m => m.Duration),
                "duration_desc" => query.OrderByDescending(m => m.Duration),
                "id_desc" => query.OrderByDescending(m => m.Id),
                _ => query.OrderBy(m => m.Id)
            };

        private static IQueryable<Musical> ApplyPaging(IQueryable<Musical> query, int page, int pageSize) =>
            query.Skip((page - 1) * pageSize).Take(pageSize);
    }
}
