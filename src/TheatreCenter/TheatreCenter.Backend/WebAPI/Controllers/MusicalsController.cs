using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;
using TheatreCenter.DTOs.Musical;
using TheatreCenter.Services.Interfaces.Services;

namespace TheatreCenter.Backend.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MusicalsController : ControllerBase
    {
        private readonly IMusicalService _musicalService;

        public MusicalsController(IMusicalService musicalService)
        {
            _musicalService = musicalService;
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            OperationId = "GetMusicalById",
            Summary = "Get musical by ID",
            Description = "Returns a single musical with the specified ID")]
        [SwaggerResponse(200, "Musical found", typeof(MusicalDTO))]
        [SwaggerResponse(404, "Musical not found")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var musical = await _musicalService.GetMusicalByIdAsync(id);
                var musicalDto = new MusicalDTO(
                    musical.Id,
                    musical.Title,
                    musical.Description,
                    musical.Duration,
                    musical.AgeRestriction,
                    musical.TheatreId
                );
                return Ok(musicalDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [SwaggerOperation(
            OperationId = "GetAllMusicals",
            Summary = "Get all musicals",
            Description = "Returns a list of all musicals")]
        [SwaggerResponse(200, "List of musicals", typeof(IEnumerable<MusicalDTO>))]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var musicals = await _musicalService.GetAllMusicalsAsync();
            var musicalDtos = musicals.Select(m => new MusicalDTO(
                m.Id,
                m.Title,
                m.Description,
                m.Duration,
                m.AgeRestriction,
                m.TheatreId
            ));
            return Ok(musicalDtos);
        }

        [HttpPost]
        [SwaggerOperation(
            OperationId = "CreateMusical",
            Summary = "Create new musical",
            Description = "Creates a new musical with the provided data")]
        [SwaggerResponse(201, "Musical created", typeof(MusicalDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateMusicalDTO createDto)
        {
            try
            {
                var musical = new Musical(
                    0,
                    createDto.Title,
                    createDto.Description,
                    createDto.Duration,
                    createDto.AgeRestriction,
                    createDto.TheatreId
                );

                var createdMusical = await _musicalService.CreateMusicalAsync(musical);
                var musicalDto = new MusicalDTO(
                    createdMusical.Id,
                    createdMusical.Title,
                    createdMusical.Description,
                    createdMusical.Duration,
                    createdMusical.AgeRestriction,
                    createdMusical.TheatreId
                );

                return CreatedAtAction(nameof(GetById), new { id = musicalDto.Id }, musicalDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            OperationId = "UpdateMusical",
            Summary = "Update musical",
            Description = "Updates an existing musical with the provided data")]
        [SwaggerResponse(200, "Musical updated", typeof(MusicalDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Musical not found")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMusicalDTO updateDto)
        {
            try
            {
                var existingMusical = await _musicalService.GetMusicalByIdAsync(id);

                existingMusical.Description = updateDto.Description;
                existingMusical.Duration = updateDto.Duration;
                existingMusical.AgeRestriction = (AgeRestriction)updateDto.AgeRestriction;
                existingMusical.TheatreId = updateDto.TheatreId;

                var updatedMusical = await _musicalService.UpdateMusicalAsync(existingMusical);
                var musicalDto = new MusicalDTO(
                    updatedMusical.Id,
                    updatedMusical.Title,
                    updatedMusical.Description,
                    updatedMusical.Duration,
                    updatedMusical.AgeRestriction,
                    updatedMusical.TheatreId
                //DateTime.Now
                );

                return Ok(musicalDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
            OperationId = "DeleteMusical",
            Summary = "Delete musical",
            Description = "Deletes a musical with the specified ID")]
        [SwaggerResponse(204, "Musical deleted")]
        [SwaggerResponse(400, "Cannot delete musical")]
        [SwaggerResponse(404, "Musical not found")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _musicalService.DeleteMusicalAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("theatre/{theatreId}")]
        [SwaggerOperation(
            OperationId = "GetMusicalsByTheatre",
            Summary = "Get musicals by theatre",
            Description = "Returns musicals for a specific theatre")]
        [SwaggerResponse(200, "List of musicals", typeof(IEnumerable<MusicalDTO>))]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByTheatre(int theatreId)
        {
            try
            {
                var musicals = await _musicalService.GetMusicalsByTheatreAsync(theatreId);
                var musicalDtos = musicals.Select(m => new MusicalDTO(
                    m.Id,
                    m.Title,
                    m.Description,
                    m.Duration,
                    m.AgeRestriction,
                    m.TheatreId
                ));
                return Ok(musicalDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("age-restriction/{ageRestriction}")]
        [SwaggerOperation(
            OperationId = "GetMusicalsByAgeRestriction",
            Summary = "Get musicals by age restriction",
            Description = "Returns musicals filtered by age restriction")]
        [SwaggerResponse(200, "List of musicals", typeof(IEnumerable<MusicalDTO>))]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByAgeRestriction(AgeRestriction ageRestriction)
        {
            try
            {
                var musicals = await _musicalService.GetMusicalsByAgeRestrictionAsync(ageRestriction);
                var musicalDtos = musicals.Select(m => new MusicalDTO(
                    m.Id,
                    m.Title,
                    m.Description,
                    m.Duration,
                    m.AgeRestriction,
                    m.TheatreId
                ));
                return Ok(musicalDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}