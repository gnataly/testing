using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TheatreCenter.Domain.Models;
using TheatreCenter.DTOs.Theme;
using TheatreCenter.Services.Interfaces.Services;

namespace TheatreCenter.Backend.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThemesController : ControllerBase
    {
        private readonly IThemeService _themeService;

        public ThemesController(IThemeService themeService)
        {
            _themeService = themeService;
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            OperationId = "GetThemeById",
            Summary = "Get theme by ID",
            Description = "Returns a single theme with the specified ID")]
        [SwaggerResponse(200, "Theme found", typeof(ThemeDTO))]
        [SwaggerResponse(404, "Theme not found")]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var theme = await _themeService.GetByIdAsync(id);
                var themeDto = new ThemeDTO(
                    theme.Id,
                    theme.Name
                );
                return Ok(themeDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [SwaggerOperation(
            OperationId = "GetAllThemes",
            Summary = "Get all themes",
            Description = "Returns a list of all themes")]
        [SwaggerResponse(200, "List of themes", typeof(IEnumerable<ThemeDTO>))]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var themes = await _themeService.GetAllAsync();
            var themeDtos = themes.Select(t => new ThemeDTO(
                t.Id,
                t.Name
            ));
            return Ok(themeDtos);
        }

        [HttpPost]
        [SwaggerOperation(
            OperationId = "CreateTheme",
            Summary = "Create new theme",
            Description = "Creates a new theme with the provided data")]
        [SwaggerResponse(201, "Theme created", typeof(ThemeDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateThemeDTO createDto)
        {
            try
            {
                var theme = new Theme(
                    0,
                    createDto.Name
                );

                var createdTheme = await _themeService.CreateAsync(theme);
                var themeDto = new ThemeDTO(
                    createdTheme.Id,
                    createdTheme.Name
                );

                return CreatedAtAction(nameof(GetById), new { id = themeDto.Id }, themeDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            OperationId = "UpdateTheme",
            Summary = "Update theme",
            Description = "Updates an existing theme with the provided data")]
        [SwaggerResponse(200, "Theme updated", typeof(ThemeDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Theme not found")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateThemeDTO updateDto)
        {
            try
            {
                var existingTheme = await _themeService.GetByIdAsync(id);

                existingTheme.Name = updateDto.Name;

                var updatedTheme = await _themeService.UpdateAsync(existingTheme);
                var themeDto = new ThemeDTO(
                    updatedTheme.Id,
                    updatedTheme.Name
                );

                return Ok(themeDto);
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
            OperationId = "DeleteTheme",
            Summary = "Delete theme",
            Description = "Deletes a theme with the specified ID")]
        [SwaggerResponse(204, "Theme deleted")]
        [SwaggerResponse(400, "Cannot delete theme")]
        [SwaggerResponse(404, "Theme not found")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _themeService.DeleteAsync(id);
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
    }
}

