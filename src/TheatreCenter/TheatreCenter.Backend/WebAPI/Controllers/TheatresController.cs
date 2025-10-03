using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TheatreCenter.Domain.Models;
using TheatreCenter.DTOs.Theatre;
using TheatreCenter.Services.Interfaces.Services;

namespace TheatreCenter.Backend.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TheatresController : ControllerBase
    {
        private readonly ITheatreService _theatreService;

        public TheatresController(ITheatreService theatreService)
        {
            _theatreService = theatreService;
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            OperationId = "GetTheatreById",
            Summary = "Get theatre by ID",
            Description = "Returns a single theatre with the specified ID")]
        [SwaggerResponse(200, "Theatre found", typeof(TheatreDTO))]
        [SwaggerResponse(404, "Theatre not found")]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var theatre = await _theatreService.GetTheatreByIdAsync(id);
                if (theatre == null) return NotFound();

                var theatreDto = new TheatreDTO(
                    theatre.Id,
                    theatre.Name,
                    theatre.AddInfo
                );
                return Ok(theatreDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [SwaggerOperation(
            OperationId = "GetAllTheatres",
            Summary = "Get all theatres",
            Description = "Returns a list of all theatres")]
        [SwaggerResponse(200, "List of theatres", typeof(IEnumerable<TheatreDTO>))]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var theatres = await _theatreService.GetAllTheatresAsync();
            var theatreDtos = theatres.Select(t => new TheatreDTO(
                t.Id,
                t.Name,
                t.AddInfo
            ));
            return Ok(theatreDtos);
        }

        [HttpPost]
        [SwaggerOperation(
            OperationId = "CreateTheatre",
            Summary = "Create new theatre",
            Description = "Creates a new theatre with the provided data")]
        [SwaggerResponse(201, "Theatre created", typeof(TheatreDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateTheatreDTO createDto)
        {
            try
            {
                var theatre = new Theatre(
                    0,
                    createDto.Name,
                    createDto.AddInfo
                );

                var createdTheatre = await _theatreService.CreateTheatreAsync(theatre);
                var theatreDto = new TheatreDTO(
                    createdTheatre.Id,
                    createdTheatre.Name,
                    createdTheatre.AddInfo
                );

                return CreatedAtAction(nameof(GetById), new { id = theatreDto.Id }, theatreDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            OperationId = "UpdateTheatre",
            Summary = "Update theatre",
            Description = "Updates an existing theatre with the provided data")]
        [SwaggerResponse(200, "Theatre updated", typeof(TheatreDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Theatre not found")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTheatreDTO updateDto)
        {
            try
            {
                var existingTheatre = await _theatreService.GetTheatreByIdAsync(id);
                if (existingTheatre == null) return NotFound();

                existingTheatre.Name = updateDto.Name;
                existingTheatre.AddInfo = updateDto.AddInfo;

                var updatedTheatre = await _theatreService.UpdateTheatreAsync(existingTheatre);
                var theatreDto = new TheatreDTO(
                    updatedTheatre.Id,
                    updatedTheatre.Name,
                    updatedTheatre.AddInfo
                );

                return Ok(theatreDto);
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
            OperationId = "DeleteTheatre",
            Summary = "Delete theatre",
            Description = "Deletes a theatre with the specified ID")]
        [SwaggerResponse(204, "Theatre deleted")]
        [SwaggerResponse(400, "Cannot delete theatre")]
        [SwaggerResponse(404, "Theatre not found")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _theatreService.DeleteTheatreAsync(id);
                return result ? NoContent() : NotFound();
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
    }
}