using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using TheatreCenter.DTOs;

namespace TheatreCenter.Services.Services
{
    public class ActorService : IActorService
    {
        private readonly ILogger<ActorService> _logger;
        private readonly IActorRepository _actorRepository;
        private readonly IAccountRepository _accountRepository;

        public ActorService(IActorRepository actorRepository, IAccountRepository accountRepository, ILogger<ActorService> logger)
        {
            _logger = logger;
            _actorRepository = actorRepository;
            _accountRepository = accountRepository;
        }

        public async Task<Actor> GetActorByIdAsync(int id, int? currentUserId = null)
        {
            _logger.LogInformation("Attempting to get actor with ID: {ActorId}", id);

            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid actor ID provided: {ActorId}", id);
                    throw new ArgumentException("Invalid actor ID");
                }

                var actor = await _actorRepository.GetByIdAsync(id);

                if (actor == null)
                {
                    _logger.LogWarning("Actor with ID {ActorId} not found", id);
                    throw new KeyNotFoundException("Actor not found");
                }

                if (currentUserId.HasValue)
                {
                    actor.IsFavorite = await _accountRepository.IsFavoriteActorAsync(currentUserId.Value, id);
                }

                _logger.LogInformation("Successfully retrieved actor with ID: {ActorId}", id);
                return actor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting actor with ID: {ActorId}", id);
                throw;
            }
        }

        public async Task<List<Actor>> GetAllActorsAsync(ActorFilter filter, int? currentUserId = null)
        {
            _logger.LogInformation("Starting to retrieve all actors with filter");

            try
            {
                var result = await _actorRepository.GetAllAsync(filter);

                if (currentUserId.HasValue)
                {
                    foreach (var actor in result)
                    {
                        actor.IsFavorite = await _accountRepository.IsFavoriteActorAsync(currentUserId.Value, actor.Id);
                    }

                    if (filter.OnlyFavorites)
                    {
                        result = result.Where(a => a.IsFavorite).ToList();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all actors");
                throw;
            }
        }

        public async Task<int> GetCountAsync(ActorFilter filter, int? currentUserId = null)
        {
            _logger.LogInformation("Starting to retrieve all actors with filter");

            try
            {
                var result = await _actorRepository.GetCountAsync(filter);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all actors");
                throw;
            }
        }

        public async Task<Actor> CreateActorAsync(Actor actor)
        {
            _logger.LogInformation("Attempting to create new actor");

            try
            {
                if (actor == null)
                {
                    _logger.LogError("Attempted to create null actor");
                    throw new ArgumentNullException(nameof(actor));
                }

                if (string.IsNullOrWhiteSpace(actor.Name))
                {
                    _logger.LogError("Attempted to create actor with empty name");
                    throw new ArgumentException("Actor name cannot be empty");
                }

                if (actor.BirthDate > DateTime.Now)
                {
                    _logger.LogError("Invalid birth date provided: {BirthDate}", actor.BirthDate);
                    throw new ArgumentException("Birth date cannot be in the future");
                }

                if (actor.Height < 100 || actor.Height > 250)
                {
                    _logger.LogError("Invalid height value provided: {Height}", actor.Height);
                    throw new ArgumentException("Invalid height value");
                }

                await _actorRepository.AddAsync(actor);
                await _actorRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully created new actor with ID: {ActorId}", actor.Id);
                return actor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating actor");
                throw;
            }
        }

        public async Task<Actor> UpdateActorAsync(Actor actor)
        {
            _logger.LogInformation("Attempting to update actor with ID: {ActorId}", actor?.Id);

            try
            {
                if (actor == null)
                {
                    _logger.LogError("Attempted to update null actor");
                    throw new ArgumentNullException(nameof(actor));
                }

                var existingActor = await _actorRepository.GetByIdAsync(actor.Id);
                if (existingActor == null)
                {
                    _logger.LogWarning("Actor with ID {ActorId} not found for update", actor.Id);
                    throw new KeyNotFoundException("Actor not found");
                }

                _logger.LogDebug("Updating actor properties for ID: {ActorId}", actor.Id);
                existingActor.Name = actor.Name;
                existingActor.VoiceType = actor.VoiceType;
                existingActor.Gender = actor.Gender;
                existingActor.BirthDate = actor.BirthDate;
                existingActor.Height = actor.Height;
                existingActor.Weight = actor.Weight;
                existingActor.AddInfo = actor.AddInfo;

                await _actorRepository.UpdateAsync(existingActor);
                await _actorRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully updated actor with ID: {ActorId}", actor.Id);
                return existingActor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating actor with ID: {ActorId}", actor?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteActorAsync(int id)
        {
            _logger.LogInformation("Attempting to delete actor with ID: {ActorId}", id);

            try
            {
                var actor = await _actorRepository.GetByIdAsync(id);
                if (actor == null)
                {
                    _logger.LogWarning("Actor with ID {ActorId} not found for deletion", id);
                    return false;
                }

                if (actor.ActorRoles.Any())
                {
                    _logger.LogError("Cannot delete actor with ID {ActorId} - has assigned roles", id);
                    throw new InvalidOperationException("Cannot delete actor with assigned roles");
                }

                await _actorRepository.RemoveAsync(actor);
                await _actorRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted actor with ID: {ActorId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting actor with ID: {ActorId}", id);
                throw;
            }
        }

        

        public async Task<bool> AddActorToRoleAsync(int actorId, int roleId)
        {
            _logger.LogInformation("Adding actor {ActorId} to role {RoleId}", actorId, roleId);

            try
            {
                var result = await _actorRepository.AddActorToRoleAsync(actorId, roleId);
                if (result)
                {
                    await _actorRepository.SaveChangesAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding actor to role");
                throw;
            }
        }

        public async Task<bool> RemoveActorFromRoleAsync(int actorId, int roleId)
        {
            _logger.LogInformation("Removing actor {ActorId} from role {RoleId}", actorId, roleId);

            try
            {
                var result = await _actorRepository.RemoveActorFromRoleAsync(actorId, roleId);
                if (result)
                {
                    await _actorRepository.SaveChangesAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing actor from role");
                throw;
            }
        }
    }
}