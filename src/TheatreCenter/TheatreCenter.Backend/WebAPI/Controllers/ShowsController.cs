using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TheatreCenter.DTOs;
using TheatreCenter.Services.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TheatreCenter.Domain.Models;
using System.ComponentModel.DataAnnotations;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Backend.WebAPI.Controllers;

[ApiController]
[Route("/api/v1/shows")]
public class ShowsController : ControllerBase
{
    private readonly IShowService _showService;
    private readonly ILogger<ShowsController> _logger;

    public ShowsController(IShowService showService, ILogger<ShowsController> logger)
    {
        _showService = showService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetAllShows",
        Summary = "Получить все показы с фильтрацией",
        Description = "Получить пагинированный список показов с возможностью фильтрации по различным параметрам, включая участие конкретного актера")]
    [SwaggerResponse(200, "Успешное получение списка показов", typeof(ShowListDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
    public async Task<IActionResult> GetAllShows(
        [FromQuery] int? musicalId,
        [FromQuery] int? actorId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] SortType sort = SortType.date_asc)
    {
        try
        {
            var filter = new ShowFilter
            {
                Page = page,
                PageSize = pageSize,
                MusicalId = musicalId,
                ActorId = actorId,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Sort = sort.ToString()
            };

            var result = await _showService.GetAllAsync(filter);

            var totalCount = result.Count();
            var items = result
                .OrderBy(a => a.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var showDtos = items.Select(s => new ShowDto
            {
                Id = s.Id,
                Date = s.Date,
                MusicalId = s.MusicalId
            }).ToList();

            var response = new ShowListDto
            {
                Items = showDtos,
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
        OperationId = "CreateShow",
        Summary = "Создать новый показ",
        Description = "Создать новый показ (только для администраторов)")]
    [SwaggerResponse(201, "Показ успешно создан", typeof(ShowDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
    public async Task<IActionResult> CreateShow([Required][FromBody] CreateShowRequestDto createShowRequestDto)
    {
        try
        {
            var show = new Show(
                id: 0,
                date: createShowRequestDto.Date,
                musicalId: createShowRequestDto.MusicalId
            );

            var createdShow = await _showService.CreateAsync(show);

            var showDto = new ShowDto
            {
                Id = createdShow.Id,
                Date = createdShow.Date,
                MusicalId = createdShow.MusicalId
            };

            return CreatedAtAction(nameof(GetShowByShowId), new { showId = showDto.Id }, showDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return BadRequest(new ErrorDto("ValidationError", ex.Message));
        }
    }

    [HttpGet("{showId}")]
    [SwaggerOperation(
        OperationId = "GetShowByShowId",
        Summary = "Получить показ по ID",
        Description = "Получить информацию о показе по его ID")]
    [SwaggerResponse(200, "Успешное получение показа", typeof(ShowDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> GetShowByShowId([Required][FromRoute] int showId)
    {
        try
        {
            var show = await _showService.GetByIdAsync(showId);

            var showDto = new ShowDto
            {
                Id = show.Id,
                Date = show.Date,
                MusicalId = show.MusicalId
            };

            return Ok(showDto);
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

    [HttpPut("{showId}")]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(
        OperationId = "UpdateShowByShowId",
        Summary = "Обновить показ",
        Description = "Обновить информацию о показе (только для администраторов)")]
    [SwaggerResponse(200, "Показ успешно обновлен", typeof(ShowDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> UpdateShowByShowId([Required][FromRoute] int showId, [Required][FromBody] UpdateShowRequestDto updateShowRequestDto)
    {
        try
        {
            var existingShow = await _showService.GetByIdAsync(showId);

            var show = new Show(
                id: showId,
                date: updateShowRequestDto.Date,
                musicalId: existingShow.MusicalId
            );

            var updatedShow = await _showService.UpdateAsync(show);

            var showDto = new ShowDto
            {
                Id = updatedShow.Id,
                Date = updatedShow.Date,
                MusicalId = updatedShow.MusicalId
            };

            return Ok(showDto);
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

    [HttpDelete("{showId}")]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(
        OperationId = "DeleteShowByShowId",
        Summary = "Удалить показ",
        Description = "Удалить показ (только для администраторов)")]
    [SwaggerResponse(204, "Показ успешно удален")]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> DeleteShowByShowId([Required][FromRoute] int showId)
    {
        try
        {
            var result = await _showService.DeleteAsync(showId);
            if (!result)
            {
                return NotFound(new ErrorDto("NotFound", "Show not found"));
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