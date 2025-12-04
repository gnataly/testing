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
    [Route("/api/v1/musicals")]
    public class MusicalsController : ControllerBase
    {
        private readonly IMusicalService _musicalService;
        private readonly ILogger<MusicalsController> _logger;

        public MusicalsController(IMusicalService musicalService, ILogger<MusicalsController> logger)
        {
            _musicalService = musicalService;
            _logger = logger;
        }

        [HttpGet]
        [SwaggerOperation(
            OperationId = "GetAllMusicals",
            Summary = "Получить все мюзиклы с фильтрацией",
            Description = "Получить пагинированный список мюзиклов с возможностью фильтрации по различным параметрам")]
        [SwaggerResponse(200, "Успешное получение списка мюзиклов", typeof(MusicalListDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
        public async Task<IActionResult> GetAllMusicals(
            [FromQuery] AgeRestriction? ageRestriction,
            [FromQuery] int? theatreId,
            [FromQuery] int? themeId,
            [FromQuery] string? search,
            [FromQuery] bool onlyFavorites = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] SortType sort = SortType.id_asc)
        {
            try
            {
                var filter = new MusicalFilter
                {
                    Page = page,
                    PageSize = pageSize,
                    Search = search,
                    AgeRestriction = ageRestriction.HasValue ? ageRestriction.Value : null,
                    TheatreId = theatreId,
                    ThemeId = themeId, 
                    OnlyFavorites = onlyFavorites,
                    Sort = sort.ToString()
                };

                var currentUser = User.FindFirst(ClaimTypes.NameIdentifier);

                int? cId = null;
                if (currentUser != null)
                    cId = int.Parse(currentUser.Value);

                var result = await _musicalService.GetAllMusicalsAsync(filter, cId);

                var totalCount = await _musicalService.GetCountAsync(filter, cId);

                var musicalDtos = result.Select(m => new MusicalDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    Duration = m.Duration,
                    AgeRestriction = m.AgeRestriction,
                    TheatreId = m.TheatreId,
                    IsFavorite = m.IsFavorite
                }).ToList();

                var response = new MusicalListDto
                {
                    Items = musicalDtos,
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
            OperationId = "CreateMusical",
            Summary = "Создать новый мюзикл",
            Description = "Создать новый мюзикл (только для администраторов)")]
        [SwaggerResponse(201, "Мюзикл успешно создан", typeof(MusicalDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
        [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
        public async Task<IActionResult> CreateMusical([Required][FromBody] CreateMusicalRequestDto createMusicalRequestDto)
        {
            try
            {
                var musical = new Musical(
                    id: 0,
                    title: createMusicalRequestDto.Title,
                    description: createMusicalRequestDto.Description,
                    duration: createMusicalRequestDto.Duration,
                    ageRestriction: createMusicalRequestDto.AgeRestriction,
                    theatreId: createMusicalRequestDto.TheatreId
                );

                var createdMusical = await _musicalService.CreateMusicalAsync(musical);

                var musicalDto = new MusicalDto
                {
                    Id = createdMusical.Id,
                    Title = createdMusical.Title,
                    Description = createdMusical.Description,
                    Duration = createdMusical.Duration,
                    AgeRestriction = createdMusical.AgeRestriction,
                    TheatreId = createdMusical.TheatreId,
                    IsFavorite = false
                };

                return CreatedAtAction(nameof(GetMusicalByMusicalId), new { musicalId = musicalDto.Id }, musicalDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                return BadRequest(new ErrorDto("ValidationError", ex.Message));
            }
        }

        [HttpGet("{musicalId}")]
        [SwaggerOperation(
            OperationId = "GetMusicalByMusicalId",
            Summary = "Получить мюзикл по ID",
            Description = "Получить информацию о мюзикле по его ID")]
        [SwaggerResponse(200, "Успешное получение мюзикла", typeof(MusicalDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
        public async Task<IActionResult> GetMusicalByMusicalId([Required][FromRoute] int musicalId)
        {
            try
            {
                var currentUser = User.FindFirst(ClaimTypes.NameIdentifier);
                Musical? musical;

                if (currentUser != null)
                    musical = await _musicalService.GetMusicalByIdAsync(musicalId, int.Parse(currentUser.Value));
                else
                    musical = await _musicalService.GetMusicalByIdAsync(musicalId);

                var musicalDto = new MusicalDto
                {
                    Id = musical.Id,
                    Title = musical.Title,
                    Description = musical.Description,
                    Duration = musical.Duration,
                    AgeRestriction = musical.AgeRestriction,
                    TheatreId = musical.TheatreId
                };

                return Ok(musicalDto);
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

        [HttpPut("{musicalId}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            OperationId = "UpdateMusicalByMusicalId",
            Summary = "Обновить мюзикл",
            Description = "Обновить информацию о мюзикле (только для администраторов)")]
        [SwaggerResponse(200, "Мюзикл успешно обновлен", typeof(MusicalDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
        [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
        [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
        public async Task<IActionResult> UpdateMusicalByMusicalId([Required][FromRoute] int musicalId, [Required][FromBody] UpdateMusicalRequestDto updateMusicalRequestDto)
        {
            try
            {
                var musical = new Musical(
                    id: musicalId,
                    title: updateMusicalRequestDto.Title,
                    description: updateMusicalRequestDto.Description,
                    duration: updateMusicalRequestDto.Duration,
                    ageRestriction: updateMusicalRequestDto.AgeRestriction,
                    theatreId: updateMusicalRequestDto.TheatreId
                );

                var updatedMusical = await _musicalService.UpdateMusicalAsync(musical);

                var musicalDto = new MusicalDto
                {
                    Id = updatedMusical.Id,
                    Title = updatedMusical.Title,
                    Description = updatedMusical.Description,
                    Duration = updatedMusical.Duration,
                    AgeRestriction = updatedMusical.AgeRestriction,
                    TheatreId = updatedMusical.TheatreId,
                    IsFavorite = updatedMusical.IsFavorite
                };

                return Ok(musicalDto);
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

        [HttpDelete("{musicalId}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            OperationId = "DeleteMusicalByMusicalId",
            Summary = "Удалить мюзикл",
            Description = "Удалить мюзикл (только для администраторов)")]
        [SwaggerResponse(204, "Мюзикл успешно удален")]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
        [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
        public async Task<IActionResult> DeleteMusicalByMusicalId([Required][FromRoute] int musicalId)
        {
            try
            {
                var result = await _musicalService.DeleteMusicalAsync(musicalId);
                if (!result)
                {
                    return NotFound(new ErrorDto("NotFound", "Musical not found"));
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