using Microsoft.EntityFrameworkCore;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            return await _context.Theatres
                .Include(t => t.Musicals)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Theatre>> GetAllAsync(TheatreFilter filter)
        {
            var query = _context.Theatres
                .Include(t => t.Musicals)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(t => t.Name.Contains(filter.Search) ||
                                        t.AddInfo.Contains(filter.Search));
            }

            query = filter.Sort switch
            {
                "name_asc" => query.OrderBy(t => t.Name),
                "name_desc" => query.OrderByDescending(t => t.Name),
                "id_desc" => query.OrderByDescending(t => t.Id),
                _ => query.OrderBy(t => t.Id)
            };

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return items;
        }

        public async Task<int> GetCountAsync(TheatreFilter filter)
        {
            var query = _context.Theatres
                .Include(t => t.Musicals)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(t => t.Name.Contains(filter.Search) ||
                                        t.AddInfo.Contains(filter.Search));
            }

            query = filter.Sort switch
            {
                "name_asc" => query.OrderBy(t => t.Name),
                "name_desc" => query.OrderByDescending(t => t.Name),
                "id_desc" => query.OrderByDescending(t => t.Id),
                _ => query.OrderBy(t => t.Id)
            };

            return await query.CountAsync();
        }




        public async Task AddAsync(Theatre theatre)
        {
            await _context.Theatres.AddAsync(theatre);
        }

        public async Task UpdateAsync(Theatre theatre)
        {
            _context.Theatres.Update(theatre);
        }

        public async Task RemoveAsync(Theatre theatre)
        {
            _context.Theatres.Remove(theatre);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
