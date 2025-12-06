using Microsoft.EntityFrameworkCore;
using TheatreCenter.Data;
using TheatreCenter.Domain;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Data.Repositories;

public class ActorRepository : IActorRepository
{
    private readonly AppDbContext _context;

    public ActorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Actor?> GetByIdAsync(int id)
    {
        return await _context.Actors
            .Include(a => a.ActorRoles)
                .ThenInclude(ar => ar.Role)
            .Include(a => a.CastMembers)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Actor>> GetAllAsync(ActorFilter filter)
    {
        var query = _context.Actors.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Search))
        {
            query = query.Where(a => a.Name.Contains(filter.Search));
        }

        if (filter.VoiceType.HasValue)
        {
            query = query.Where(a => a.VoiceType == filter.VoiceType.Value);
        }

        if (filter.Gender.HasValue)
        {
            query = query.Where(a => a.Gender == filter.Gender.Value);
        }

        query = filter.Sort switch
        {
            "name_asc" => query.OrderBy(a => a.Name),
            "name_desc" => query.OrderByDescending(a => a.Name),
            "birthDate_asc" => query.OrderBy(a => a.BirthDate),
            "birthDate_desc" => query.OrderByDescending(a => a.BirthDate),
            "id_desc" => query.OrderByDescending(a => a.Id),
            _ => query.OrderBy(a => a.Id)
        };

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return items;
    }

    public async Task<int> GetCountAsync(ActorFilter filter)
    {
        var query = _context.Actors.AsQueryable();

        if (!string.IsNullOrEmpty(filter.Search))
        {
            query = query.Where(a => a.Name.Contains(filter.Search));
        }

        if (filter.VoiceType.HasValue)
        {
            query = query.Where(a => a.VoiceType == filter.VoiceType.Value);
        }

        if (filter.Gender.HasValue)
        {
            query = query.Where(a => a.Gender == filter.Gender.Value);
        }

        query = filter.Sort switch
        {
            "name_asc" => query.OrderBy(a => a.Name),
            "name_desc" => query.OrderByDescending(a => a.Name),
            "birthDate_asc" => query.OrderBy(a => a.BirthDate),
            "birthDate_desc" => query.OrderByDescending(a => a.BirthDate),
            "id_desc" => query.OrderByDescending(a => a.Id),
            _ => query.OrderBy(a => a.Id)
        };


        return await query.CountAsync();
    }

    public async Task AddAsync(Actor actor)
    {
        await _context.Actors.AddAsync(actor);
    }

    public async Task UpdateAsync(Actor actor)
    {
        _context.Actors.Update(actor);
    }

    public async Task RemoveAsync(Actor actor)
    {
        _context.Actors.Remove(actor);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Actor>> GetByVoiceTypeAsync(VoiceType voiceType)
    {
        return await _context.Actors
            .Where(a => a.VoiceType == voiceType)
            .ToListAsync();
    }

    public async Task<IEnumerable<Actor>> GetActorsByRoleAsync(int roleId)
    {
        return await _context.Actors
            .Where(a => a.ActorRoles.Any(ar => ar.RoleId == roleId))
            .ToListAsync();
    }

    public async Task<bool> AddActorToRoleAsync(int actorId, int roleId)
    {
        var existing = await _context.ActorRoles
            .FirstOrDefaultAsync(ar => ar.ActorId == actorId && ar.RoleId == roleId);

        if (existing != null) return false;

        var actorRole = new ActorRole(actorId, roleId);
        await _context.ActorRoles.AddAsync(actorRole);
        return true;
    }

    public async Task<bool> RemoveActorFromRoleAsync(int actorId, int roleId)
    {
        var actorRole = await _context.ActorRoles
            .FirstOrDefaultAsync(ar => ar.ActorId == actorId && ar.RoleId == roleId);

        if (actorRole == null) return false;

        _context.ActorRoles.Remove(actorRole);
        return true;
    }
}