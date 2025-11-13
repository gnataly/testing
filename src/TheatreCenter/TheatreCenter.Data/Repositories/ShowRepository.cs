using Microsoft.EntityFrameworkCore;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheatreCenter.Data.Repositories;

public class ShowRepository : IShowRepository
{
    private readonly AppDbContext _context;

    public ShowRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Show?> GetByIdAsync(int id)
    {
        return await _context.Shows.FindAsync(id);
    }

    public async Task<IEnumerable<Show>> GetAllAsync()
    {
        return await _context.Shows.ToListAsync();
    }

    public async Task AddAsync(Show show)
    {
        _context.Shows.AddAsync(show);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Show show)
    {
        _context.Shows.Update(show);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Show show)
    {
        _context.Shows.Remove(show);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Show>> GetByMusicalIdAsync(int musicalId)
    {
        return await _context.Shows
            .Where(s => s.MusicalId == musicalId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Show>> GetUpcomingShowsAsync()
    {
        return await _context.Shows
            .Where(s => s.Date >= DateTime.UtcNow)
            .OrderBy(s => s.Date)
            .ToListAsync();
    }
}