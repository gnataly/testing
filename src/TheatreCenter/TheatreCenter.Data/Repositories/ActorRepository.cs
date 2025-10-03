using TheatreCenter.Data;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheatreCenter.Domain.Enums;
using Microsoft.Extensions.Logging;


public class ActorRepository : IActorRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ActorRepository> _logger;

    public ActorRepository(AppDbContext context, ILogger<ActorRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Actor?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Querying actor by ID: {ActorId}", id);
        try
        {
            var actor = await _context.Actors.FindAsync(id);
            _logger.LogDebug("Actor with ID {ActorId} was {Result}",
                id, actor == null ? "not found" : "found");
            return actor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching actor with ID: {ActorId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Actor>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all actors");
        try
        {
            var actors = await _context.Actors.ToListAsync();
            _logger.LogInformation("Successfully retrieved {Count} actors", actors.Count);
            return actors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all actors");
            throw;
        }
    }

    public async Task AddAsync(Actor actor)
    {
        _logger.LogInformation("Adding new actor with ID: {ActorId}", actor.Id);
        try
        {
            await _context.Actors.AddAsync(actor);
            await _context.SaveChangesAsync();
            _logger.LogDebug("Actor with ID {ActorId} added to context", actor.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding actor with ID: {ActorId}", actor.Id);
            throw;
        }
    }

    public async Task UpdateAsync(Actor actor)
    {
        _logger.LogInformation("Updating actor with ID: {ActorId}", actor.Id);
        try
        {
            _context.Actors.Update(actor);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully updated actor with ID: {ActorId}", actor.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating actor with ID: {ActorId}", actor.Id);
            throw;
        }
    }

    public async Task RemoveAsync(Actor actor)
    {
        _logger.LogInformation("Removing actor with ID: {ActorId}", actor.Id);
        try
        {
            _context.Actors.Remove(actor);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully removed actor with ID: {ActorId}", actor.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing actor with ID: {ActorId}", actor.Id);
            throw;
        }
    }

    public async Task SaveChangesAsync()
    {
        try
        {
            _logger.LogDebug("Saving changes to database");
            await _context.SaveChangesAsync();
            _logger.LogDebug("Changes saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving changes");
            throw;
        }
    }

    public async Task<IEnumerable<Actor>> GetByVoiceTypeAsync(VoiceType voiceType)
    {
        _logger.LogInformation("Querying actors by voice type: {VoiceType}", voiceType);
        try
        {
            var actors = await _context.Actors
                .Where(a => a.VoiceType == voiceType)
                .ToListAsync();
            _logger.LogInformation("Found {Count} actors with voice type {VoiceType}",
                actors.Count, voiceType);
            return actors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while querying actors by voice type: {VoiceType}",
                voiceType);
            throw;
        }
    }
}

//public class ActorRepository : IActorRepository
//{
//    private readonly AppDbContext _context;

//    public ActorRepository(AppDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<Actor?> GetByIdAsync(int id)
//    {
//        return await _context.Actors.FindAsync(id);
//    }

//    public async Task<IEnumerable<Actor>> GetAllAsync()
//    {
//        return await _context.Actors.ToListAsync();
//    }

//    public async Task AddAsync(Actor actor)
//    {
//        await _context.Actors.AddAsync(actor);
//    }

//    public async Task UpdateAsync(Actor actor)
//    {
//        _context.Actors.Update(actor);
//        await _context.SaveChangesAsync();
//    }

//    public async Task RemoveAsync(Actor actor)
//    {
//        _context.Actors.Remove(actor);
//        await _context.SaveChangesAsync();
//    }

//    public async Task SaveChangesAsync()
//    {
//        //try
//        //{
//        //    await _context.SaveChangesAsync();
//        //}
//        //catch (DbUpdateException ex)
//        //{
//        //    Console.WriteLine(ex.InnerException?.Message);
//        //    return StatusCode(500, "Ошибка при сохранении: " + ex.InnerException?.Message);
//        //}
//        await _context.SaveChangesAsync();
//    }

//    public async Task<IEnumerable<Actor>> GetByVoiceTypeAsync(VoiceType voiceType)
//    {
//        return await _context.Actors.Where(a => a.VoiceType == voiceType).ToListAsync();
//    }
//}
