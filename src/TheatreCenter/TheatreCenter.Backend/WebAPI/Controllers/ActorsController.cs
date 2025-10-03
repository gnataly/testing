using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using TheatreCenter.Domain.Models;
using TheatreCenter.DTOs.Actor;
using TheatreCenter.Services.Interfaces.Services;

namespace TheatreCenter.Backend.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActorsController : ControllerBase
    {
        private readonly IActorService _actorService;
        private readonly ILogger<ActorsController> _logger;

        public ActorsController(IActorService actorService, ILogger<ActorsController> logger)
        {
            _actorService = actorService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Starting GetById request for actor ID: {ActorId}", id);

            try
            {
                var actor = await _actorService.GetActorByIdAsync(id);

                _logger.LogDebug("Mapping actor with ID {ActorId} to DTO", id);
                var actorDto = new ActorDTO(
                    actor.Id,
                    actor.Name,
                    actor.VoiceType,
                    actor.Gender,
                    actor.BirthDate,
                    actor.Height,
                    actor.Weight,
                    actor.AddInfo
                );

                _logger.LogInformation("Successfully processed GetById request for actor ID: {ActorId}", id);
                return Ok(actorDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Actor with ID {ActorId} not found", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GetById request for actor ID: {ActorId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Starting GetAllActors request");

            try
            {
                var actors = await _actorService.GetAllActorsAsync();

                _logger.LogDebug("Mapping {ActorCount} actors to DTOs", actors.Count());
                var actorDtos = actors.Select(a => new ActorDTO(
                    a.Id,
                    a.Name,
                    a.VoiceType,
                    a.Gender,
                    a.BirthDate,
                    a.Height,
                    a.Weight,
                    a.AddInfo
                ));

                _logger.LogInformation("Successfully processed GetAllActors request, returning {ActorCount} actors", actorDtos.Count());
                return Ok(actorDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GetAllActors request");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateActorDTO createDto)
        {
            _logger.LogInformation("Starting CreateActor request");

            try
            {
                _logger.LogDebug("Mapping CreateActorDTO to Actor entity");
                var actor = new Actor(
                    0,
                    createDto.Name,
                    createDto.VoiceType,
                    createDto.Gender,
                    createDto.BirthDate,
                    createDto.Height,
                    createDto.Weight,
                    createDto.AddInfo
                );

                var createdActor = await _actorService.CreateActorAsync(actor);

                _logger.LogDebug("Mapping created actor with ID {ActorId} to DTO", createdActor.Id);
                var actorDto = new ActorDTO(
                    createdActor.Id,
                    createdActor.Name,
                    createdActor.VoiceType,
                    createdActor.Gender,
                    createdActor.BirthDate,
                    createdActor.Height,
                    createdActor.Weight,
                    createdActor.AddInfo
                );

                _logger.LogInformation("Successfully created actor with ID: {ActorId}", createdActor.Id);
                return CreatedAtAction(nameof(GetById), new { id = actorDto.Id }, actorDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CreateActor request");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateActorDTO updateDto)
        {
            _logger.LogInformation("Starting UpdateActor request for actor ID: {ActorId}", id);

            try
            {
                _logger.LogDebug("Fetching existing actor with ID: {ActorId}", id);
                var existingActor = await _actorService.GetActorByIdAsync(id);

                _logger.LogDebug("Updating fields for actor ID: {ActorId}", id);
                //existingActor.Height = updateDto.Height;
                //existingActor.Weight = updateDto.Weight;
                //existingActor.AddInfo = updateDto.AddInfo;
                //existingActor.VoiceType = updateDto.VoiceType;
                existingActor.Name = updateDto.Name;
                existingActor.VoiceType = updateDto.VoiceType;
                existingActor.Gender = updateDto.Gender;
                existingActor.BirthDate = updateDto.BirthDate;
                existingActor.Height = updateDto.Height;
                existingActor.Weight = updateDto.Weight;
                existingActor.AddInfo = updateDto.AddInfo;

                var updatedActor = await _actorService.UpdateActorAsync(existingActor);

                _logger.LogDebug("Mapping updated actor with ID {ActorId} to DTO", id);
                var actorDto = new ActorDTO(
                    updatedActor.Id,
                    updatedActor.Name,
                    updatedActor.VoiceType,
                    updatedActor.Gender,
                    updatedActor.BirthDate,
                    updatedActor.Height,
                    updatedActor.Weight,
                    updatedActor.AddInfo
                );

                _logger.LogInformation("Successfully updated actor with ID: {ActorId}", id);
                return Ok(actorDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Actor with ID {ActorId} not found for update", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing UpdateActor request for actor ID: {ActorId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Starting DeleteActor request for actor ID: {ActorId}", id);

            try
            {
                await _actorService.DeleteActorAsync(id);
                _logger.LogInformation("Successfully deleted actor with ID: {ActorId}", id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Actor with ID {ActorId} not found for deletion", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing DeleteActor request for actor ID: {ActorId}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}