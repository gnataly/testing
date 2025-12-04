using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TheatreCenter.DTOs;
using TheatreCenter.Services.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TheatreCenter.Domain.Models;
using System.ComponentModel.DataAnnotations;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Backend.WebAPI.Controllers
{
    [ApiController]
    [Route("/api/v1/themes")]
    public class ThemesController : ControllerBase
    {
        private readonly IThemeService _themeService;
        private readonly ILogger<ThemesController> _logger;

        public ThemesController(IThemeService themeService, ILogger<ThemesController> logger)
        {
            _themeService = themeService;
            _logger = logger;
        }

        [HttpGet]
        [SwaggerOperation(
            OperationId = "GetAllThemes",
            Summary = "Получить все тематики с фильтрацией",
            Description = "Получить пагинированный список тематик с возможностью фильтрации")]
        [SwaggerResponse(200, "Успешное получение списка тематик", typeof(ThemeListDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
        public async Task<IActionResult> GetAllThemes(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] SortType sort = SortType.id_asc)
        {
            try
            {
                var filter = new ThemeFilter
                {
                    Page = page,
                    PageSize = pageSize,
                    Search = search,
                    Sort = sort.ToString()
                };

                var result = await _themeService.GetAllAsync(filter);

                var totalCount = result.Count();
                var items = result
                    .OrderBy(a => a.Id)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var themeDtos = items.Select(t => new ThemeDto
                {
                    Id = t.Id,
                    Name = t.Name
                }).ToList();

                var response = new ThemeListDto
                {
                    Items = themeDtos,
                    Pagination = new PaginationDto
                    {
                        Page = filter.Page,
                        PageSize = filter.PageSize,
                        TotalCount = totalCount
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            OperationId = "CreateTheme",
            Summary = "Создать новую тематику",
            Description = "Создать новую тематику (только для администраторов)")]
        [SwaggerResponse(201, "Тематика успешно создана", typeof(ThemeDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
        [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
        public async Task<IActionResult> CreateTheme([Required][FromBody] CreateThemeRequestDto createThemeRequestDto)
        {
            try
            {
                var theme = new Theme(
                    id: 0,
                    name: createThemeRequestDto.Name
                );

                var createdTheme = await _themeService.CreateAsync(theme);

                var themeDto = new ThemeDto
                {
                    Id = createdTheme.Id,
                    Name = createdTheme.Name
                };

                return CreatedAtAction(nameof(GetThemeByThemeId), new { themeId = themeDto.Id }, themeDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                return BadRequest(new ErrorDto("ValidationError", ex.Message));
            }
        }

        [HttpGet("{themeId}")]
        [SwaggerOperation(
            OperationId = "GetThemeByThemeId",
            Summary = "Получить тематику по ID",
            Description = "Получить информацию о тематике по ее ID")]
        [SwaggerResponse(200, "Успешное получение тематики", typeof(ThemeDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
        public async Task<IActionResult> GetThemeByThemeId([Required][FromRoute] int themeId)
        {
            try
            {
                var theme = await _themeService.GetByIdAsync(themeId);

                var themeDto = new ThemeDto
                {
                    Id = theme.Id,
                    Name = theme.Name
                };

                return Ok(themeDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorDto("NotFound", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
            }
        }

        [HttpPut("{themeId}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            OperationId = "UpdateThemeByThemeId",
            Summary = "Обновить тематику",
            Description = "Обновить информацию о тематике (только для администраторов)")]
        [SwaggerResponse(200, "Тематика успешно обновлена", typeof(ThemeDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
        [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
        [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
        public async Task<IActionResult> UpdateThemeByThemeId([Required][FromRoute] int themeId, [Required][FromBody] UpdateThemeRequestDto updateThemeRequestDto)
        {
            try
            {
                var theme = new Theme(
                    id: themeId,
                    name: updateThemeRequestDto.Name
                );

                var updatedTheme = await _themeService.UpdateAsync(theme);

                var themeDto = new ThemeDto
                {
                    Id = updatedTheme.Id,
                    Name = updatedTheme.Name
                };

                return Ok(themeDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorDto("NotFound", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                return BadRequest(new ErrorDto("ValidationError", ex.Message));
            }
        }

        [HttpDelete("{themeId}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            OperationId = "DeleteThemeByThemeId",
            Summary = "Удалить тематику",
            Description = "Удалить тематику (только для администраторов)")]
        [SwaggerResponse(204, "Тематика успешно удалена")]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
        [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
        public async Task<IActionResult> DeleteThemeByThemeId([Required][FromRoute] int themeId)
        {
            try
            {
                var result = await _themeService.DeleteAsync(themeId);
                if (!result)
                {
                    return NotFound(new ErrorDto("NotFound", "Theme not found"));
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorDto("ValidationError", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
            }
        }
    }
}