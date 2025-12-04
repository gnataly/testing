using Microsoft.EntityFrameworkCore;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Data.Repositories
{
    public class ShowRepository : IShowRepository
    {
        private readonly AppDbContext _context;

        public ShowRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Show?> GetByIdAsync(int id)
        {
            return await _context.Shows
                .Include(s => s.Musical)
                    .ThenInclude(m => m.Theatre)
                .Include(s => s.CastMembers)
                    .ThenInclude(cm => cm.Actor)
                .Include(s => s.CastMembers)
                    .ThenInclude(cm => cm.Role)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Show>> GetAllAsync(ShowFilter filter)
        {
            var query = _context.Shows
                .Include(s => s.Musical)
                    .ThenInclude(m => m.Theatre)
                .Include(s => s.CastMembers)
                    .ThenInclude(cm => cm.Actor)
                .AsQueryable();

            if (filter.MusicalId.HasValue)
            {
                query = query.Where(s => s.MusicalId == filter.MusicalId.Value);
            }

            if (filter.ActorId.HasValue)
            {
                query = query.Where(s => s.CastMembers.Any(cm => cm.ActorId == filter.ActorId.Value));
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(s => s.Date >= filter.DateFrom.Value);
            }

            if (filter.DateTo.HasValue)
            {
                query = query.Where(s => s.Date <= filter.DateTo.Value);
            }

            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(s => s.Musical.Title.Contains(filter.Search));
            }

            query = filter.Sort switch
            {
                "date_asc" => query.OrderBy(s => s.Date),
                "date_desc" => query.OrderByDescending(s => s.Date),
                "id_desc" => query.OrderByDescending(s => s.Id),
                _ => query.OrderBy(s => s.Id)
            };

            return await query.ToListAsync();
        }

        public async Task AddAsync(Show show)
        {
            await _context.Shows.AddAsync(show);
        }

        public async Task UpdateAsync(Show show)
        {
            _context.Shows.Update(show);
        }

        public async Task RemoveAsync(Show show)
        {
            _context.Shows.Remove(show);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Show>> GetByMusicalIdAsync(int musicalId)
        {
            return await _context.Shows
                .Where(s => s.MusicalId == musicalId)
                .Include(s => s.Musical)
                .Include(s => s.CastMembers)
                .ToListAsync();
        }

        public async Task<IEnumerable<Show>> GetUpcomingShowsAsync()
        {
            return await _context.Shows
                .Where(s => s.Date >= DateTime.Now)
                .Include(s => s.Musical)
                .OrderBy(s => s.Date)
                .ToListAsync();
        }
    }
}