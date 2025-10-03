using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TheatreCenter.Domain.Models;
using TheatreCenter.DTOs.Show;
using TheatreCenter.Services.Interfaces.Services;

namespace TheatreCenter.Backend.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShowsController : ControllerBase
    {
        private readonly IShowService _showService;


        public ShowsController(IShowService showService)
        {
            _showService = showService;
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            OperationId = "GetShowById",
            Summary = "Get show by ID",
            Description = "Returns a single show with the specified ID")]
        [SwaggerResponse(200, "Show found", typeof(ShowDTO))]
        [SwaggerResponse(404, "Show not found")]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var show = await _showService.GetByIdAsync(id);
                var showDto = new ShowDTO(
                    show.Id,
                    show.Date,
                    show.MusicalId

                );
                return Ok(showDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [SwaggerOperation(
            OperationId = "GetAllShows",
            Summary = "Get all shows",
            Description = "Returns a list of all shows")]
        [SwaggerResponse(200, "List of shows", typeof(IEnumerable<ShowDTO>))]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var shows = await _showService.GetAllAsync();
            var showDtos = shows.Select(s => new ShowDTO(
                s.Id,
                s.Date,
                s.MusicalId
            ));
            return Ok(showDtos);
        }

        [HttpPost]
        [SwaggerOperation(
            OperationId = "CreateShow",
            Summary = "Create new show",
            Description = "Creates a new show with the provided data")]
        [SwaggerResponse(201, "Show created", typeof(ShowDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateShowDTO createDto)
        {
            try
            {
                var show = new Show(
                    0,
                    createDto.Date,
                    createDto.MusicalId
                );

                var createdShow = await _showService.CreateAsync(show);
                var showDto = new ShowDTO(
                    createdShow.Id,
                    createdShow.Date,
                    createdShow.MusicalId
                );

                return CreatedAtAction(nameof(GetById), new { id = showDto.Id }, showDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            OperationId = "UpdateShow",
            Summary = "Update show",
            Description = "Updates an existing show with the provided data")]
        [SwaggerResponse(200, "Show updated", typeof(ShowDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Show not found")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateShowDTO updateDto)
        {
            try
            {
                var existingShow = await _showService.GetByIdAsync(id);

                existingShow.Date = updateDto.Date;

                var updatedShow = await _showService.UpdateAsync(existingShow);
                var showDto = new ShowDTO(
                    updatedShow.Id,
                    updatedShow.Date,
                    updatedShow.MusicalId
                );

                return Ok(showDto);
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
            OperationId = "DeleteShow",
            Summary = "Delete show",
            Description = "Deletes a show with the specified ID")]
        [SwaggerResponse(204, "Show deleted")]
        [SwaggerResponse(400, "Cannot delete show")]
        [SwaggerResponse(404, "Show not found")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _showService.DeleteAsync(id);
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

        [HttpGet("musical/{musicalId}")]
        [SwaggerOperation(
            OperationId = "GetShowsByMusical",
            Summary = "Get shows by musical",
            Description = "Returns shows for a specific musical")]
        [SwaggerResponse(200, "List of shows", typeof(IEnumerable<ShowDTO>))]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByMusical(int musicalId)
        {
            try
            {
                var shows = await _showService.GetByMusicalIdAsync(musicalId);
                var showDtos = shows.Select(s => new ShowDTO(
                    s.Id,
                    s.Date,
                    s.MusicalId
                ));
                return Ok(showDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}