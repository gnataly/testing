using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheatreCenter.Services.Services
{
    public class ActorService : IActorService
    {
        private readonly ILogger<ActorService> _logger;
        private readonly IActorRepository _actorRepository;

        public ActorService(IActorRepository actorRepository, ILogger<ActorService> logger)
        {
            _logger = logger;
            _actorRepository = actorRepository;
        }

        public async Task<Actor> GetActorByIdAsync(int id)
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
                    throw new ArgumentNullException("Actor not found");
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

        public async Task<IEnumerable<Actor>> GetAllActorsAsync()
        {
            _logger.LogInformation("Starting to retrieve all actors");

            try
            {
                var actors = await _actorRepository.GetAllAsync();
                _logger.LogInformation("Successfully retrieved {ActorCount} actors", actors.Count());
                return actors;
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

        public async Task<IEnumerable<Actor>> GetActorsByVoiceTypeAsync(VoiceType voiceType)
        {
            _logger.LogInformation("Attempting to get actors by voice type: {VoiceType}", voiceType);

            try
            {
                var actors = await _actorRepository.GetByVoiceTypeAsync(voiceType);
                _logger.LogInformation("Found {ActorCount} actors with voice type {VoiceType}",
                    actors.Count(), voiceType);
                return actors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting actors by voice type: {VoiceType}", voiceType);
                throw;
            }
        }
    }
}