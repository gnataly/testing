using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TheatreCenter.DTOs;
using TheatreCenter.Services.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TheatreCenter.Domain.Models;
using System.ComponentModel.DataAnnotations;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Services.Services;

namespace TheatreCenter.Backend.WebAPI.Controllers
{
    [ApiController]
    [Route("/api/v1/theatres")]
    public class TheatresController : ControllerBase
    {
        private readonly ITheatreService _theatreService;
        private readonly ILogger<TheatresController> _logger;

        public TheatresController(ITheatreService theatreService, ILogger<TheatresController> logger)
        {
            _theatreService = theatreService;
            _logger = logger;
        }

        [HttpGet]
        [SwaggerOperation(
            OperationId = "GetAllTheatres",
            Summary = "Получить все театры с фильтрацией",
            Description = "Получить пагинированный список театров с возможностью фильтрации по различным параметрам")]
        [SwaggerResponse(200, "Успешное получение списка театров", typeof(TheatreListDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
        public async Task<IActionResult> GetAllTheatres(
            [FromQuery] string? search,
            [FromQuery] bool onlyFavorites = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] SortType sort = SortType.id_asc)
        {
            try
            {
                var filter = new TheatreFilter
                {
                    Page = page,
                    PageSize = pageSize,
                    Search = search,
                    OnlyFavorites = onlyFavorites,
                    Sort = sort.ToString()
                };

                var currentUser = User.FindFirst(ClaimTypes.NameIdentifier);

                int? cId = null;
                if (currentUser != null)
                    cId = int.Parse(currentUser.Value);
                
                var result = await _theatreService.GetAllTheatresAsync(filter, cId);

                var totalCount = await _theatreService.GetCountAsync(filter, cId);

                var theatreDtos = result.Select(t => new TheatreDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    AddInfo = t.AddInfo,
                    IsFavorite = t.IsFavorite
                }).ToList();

                var response = new TheatreListDto
                {
                    Items = theatreDtos,
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
            OperationId = "CreateTheatre",
            Summary = "Создать новый театр",
            Description = "Создать новый театр (только для администраторов)")]
        [SwaggerResponse(201, "Театр успешно создан", typeof(TheatreDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
        [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
        public async Task<IActionResult> CreateTheatre([Required][FromBody] CreateTheatreRequestDto createTheatreRequestDto)
        {
            try
            {
                var theatre = new Theatre(
                    id: 0,
                    name: createTheatreRequestDto.Name,
                    addInfo: createTheatreRequestDto.AddInfo
                );

                var createdTheatre = await _theatreService.CreateTheatreAsync(theatre);

                var theatreDto = new TheatreDto
                {
                    Id = createdTheatre.Id,
                    Name = createdTheatre.Name,
                    AddInfo = createdTheatre.AddInfo
                };

                return CreatedAtAction(nameof(GetTheatreByTheatreId), new { theatreId = theatreDto.Id }, theatreDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                return BadRequest(new ErrorDto("ValidationError", ex.Message));
            }
        }

        [HttpGet("{theatreId}")]
        [SwaggerOperation(
            OperationId = "GetTheatreByTheatreId",
            Summary = "Получить театр по ID",
            Description = "Получить информацию о театре по его ID")]
        [SwaggerResponse(200, "Успешное получение театра", typeof(TheatreDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
        public async Task<IActionResult> GetTheatreByTheatreId([Required][FromRoute] int theatreId)
        {
            try
            {
                var currentUser = User.FindFirst(ClaimTypes.NameIdentifier);
                Theatre? theatre;

                if (currentUser != null)
                    theatre = await _theatreService.GetTheatreByIdAsync(theatreId, int.Parse(currentUser.Value));
                else
                    theatre = await _theatreService.GetTheatreByIdAsync(theatreId);

                var theatreDto = new TheatreDto
                {
                    Id = theatre.Id,
                    Name = theatre.Name,
                    AddInfo = theatre.AddInfo,
                    IsFavorite = theatre.IsFavorite
                };

                return Ok(theatreDto);
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

        [HttpPut("{theatreId}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            OperationId = "UpdateTheatreByTheatreId",
            Summary = "Обновить театр",
            Description = "Обновить информацию о театре (только для администраторов)")]
        [SwaggerResponse(200, "Театр успешно обновлен", typeof(TheatreDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
        [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
        [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
        public async Task<IActionResult> UpdateTheatreByTheatreId([Required][FromRoute] int theatreId, [Required][FromBody] UpdateTheatreRequestDto updateTheatreRequestDto)
        {
            try
            {
                var theatre = new Theatre(
                    id: theatreId,
                    name: updateTheatreRequestDto.Name,
                    addInfo: updateTheatreRequestDto.AddInfo
                );

                var updatedTheatre = await _theatreService.UpdateTheatreAsync(theatre);

                var theatreDto = new TheatreDto
                {
                    Id = updatedTheatre.Id,
                    Name = updatedTheatre.Name,
                    AddInfo = updatedTheatre.AddInfo,
                    IsFavorite = updatedTheatre.IsFavorite
                };

                return Ok(theatreDto);
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

        [HttpDelete("{theatreId}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            OperationId = "DeleteTheatreByTheatreId",
            Summary = "Удалить театр",
            Description = "Удалить театр (только для администраторов)")]
        [SwaggerResponse(204, "Театр успешно удален")]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
        [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
        public async Task<IActionResult> DeleteTheatreByTheatreId([Required][FromRoute] int theatreId)
        {
            try
            {
                var result = await _theatreService.DeleteTheatreAsync(theatreId);
                if (!result)
                {
                    return NotFound(new ErrorDto("NotFound", "Theatre not found"));
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
            }
        }
    }
}