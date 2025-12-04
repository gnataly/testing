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
[Route("/api/v1/actors")]
public class ActorsController : ControllerBase
{
    private readonly IActorService _actorService;
    private readonly ILogger<ActorsController> _logger;

    public ActorsController(IActorService actorService, ILogger<ActorsController> logger)
    {
        _actorService = actorService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetAllActors",
        Summary = "Получить всех актеров с фильтрацией",
        Description = "Получить пагинированный список актеров с возможностью фильтрации по различным параметрам")]
    [SwaggerResponse(200, "Успешное получение списка актеров", typeof(ActorListDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
    public async Task<IActionResult> GetAllActors(
        [FromQuery] VoiceType? voiceType,
        [FromQuery] Gender? gender,
        [FromQuery] string? search,
        [FromQuery] bool onlyFavorites = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] SortType sort = SortType.id_asc)
    {
        try
        {
            var filter = new ActorFilter
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                VoiceType = voiceType.HasValue ? voiceType.Value : null,
                Gender = gender.HasValue ? gender.Value : null,
                OnlyFavorites = onlyFavorites,
                Sort = sort.ToString()
            };

            var currentUser = User.FindFirst(ClaimTypes.NameIdentifier);

            int? cId = null;
            if (currentUser != null)
                cId = int.Parse(currentUser.Value);

            var result = await _actorService.GetAllActorsAsync(filter, cId);

            var totalCount = await _actorService.GetCountAsync(filter, cId);

            var actorDtos = result.Select(a => new ActorDto
            {
                Id = a.Id,
                Name = a.Name,
                VoiceType = a.VoiceType,
                Gender = a.Gender,
                BirthDate = a.BirthDate,
                Height = a.Height,
                Weight = a.Weight,
                AddInfo = a.AddInfo,
                IsFavorite = a.IsFavorite
            }).ToList();

            var response = new ActorListDto
            {
                Items = actorDtos,
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
        OperationId = "CreateActor",
        Summary = "Создать нового актера",
        Description = "Создать нового актера (только для администраторов)")]
    [SwaggerResponse(201, "Актер успешно создан", typeof(ActorDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
    public async Task<IActionResult> CreateActor([Required][FromBody] CreateActorRequestDto createActorRequestDto)
    {
        try
        {
            var actor = new Actor(
                id: 0,
                name: createActorRequestDto.Name,
                voiceType: createActorRequestDto.VoiceType,
                gender: createActorRequestDto.Gender,
                birthDate: createActorRequestDto.BirthDate,
                height: createActorRequestDto.Height,
                weight: createActorRequestDto.Weight,
                addInfo: createActorRequestDto.AddInfo
            );

            var createdActor = await _actorService.CreateActorAsync(actor);

            var actorDto = new ActorDto
            {
                Id = createdActor.Id,
                Name = createdActor.Name,
                VoiceType = createdActor.VoiceType,
                Gender = createdActor.Gender,
                BirthDate = createdActor.BirthDate,
                Height = createdActor.Height,
                Weight = createdActor.Weight,
                AddInfo = createdActor.AddInfo,
                IsFavorite = false
            };

            return CreatedAtAction(nameof(GetActorByActorId), new { actorId = actorDto.Id }, actorDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return BadRequest(new ErrorDto("ValidationError", ex.Message));
        }
    }

    [HttpGet("{actorId}")]
    [SwaggerOperation(
        OperationId = "GetActorByActorId",
        Summary = "Получить актера по ID",
        Description = "Получить информацию об актере по его ID")]
    [SwaggerResponse(200, "Успешное получение актера", typeof(ActorDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> GetActorByActorId([Required][FromRoute] int actorId)
    {
        try
        {
            var currentUser = User.FindFirst(ClaimTypes.NameIdentifier);
            Actor? actor;

            if (currentUser != null)
                actor = await _actorService.GetActorByIdAsync(actorId, int.Parse(currentUser.Value));
            else 
                actor = await _actorService.GetActorByIdAsync(actorId);

            var actorDto = new ActorDto
            {
                Id = actor.Id,
                Name = actor.Name,
                VoiceType = actor.VoiceType,
                Gender = actor.Gender,
                BirthDate = actor.BirthDate,
                Height = actor.Height,
                Weight = actor.Weight,
                AddInfo = actor.AddInfo,
                IsFavorite = actor.IsFavorite
            };

            return Ok(actorDto);
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

    [HttpPut("{actorId}")]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(
        OperationId = "UpdateActorByActorId",
        Summary = "Обновить актера",
        Description = "Обновить информацию об актере (только для администраторов)")]
    [SwaggerResponse(200, "Актер успешно обновлен", typeof(ActorDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> UpdateActorByActorId([Required][FromRoute] int actorId, [Required][FromBody] UpdateActorRequestDto updateActorRequestDto)
    {
        try
        {
            var actor = new Actor(
                id: actorId,
                name: updateActorRequestDto.Name,
                voiceType: updateActorRequestDto.VoiceType,
                gender: updateActorRequestDto.Gender,
                birthDate: updateActorRequestDto.BirthDate,
                height: updateActorRequestDto.Height,
                weight: updateActorRequestDto.Weight,
                addInfo: updateActorRequestDto.AddInfo
            );

            var updatedActor = await _actorService.UpdateActorAsync(actor);

            var actorDto = new ActorDto
            {
                Id = updatedActor.Id,
                Name = updatedActor.Name,
                VoiceType = updatedActor.VoiceType,
                Gender = updatedActor.Gender,
                BirthDate = updatedActor.BirthDate,
                Height = updatedActor.Height,
                Weight = updatedActor.Weight,
                AddInfo = updatedActor.AddInfo,
                IsFavorite = updatedActor.IsFavorite
            };

            return Ok(actorDto);
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

    [HttpDelete("{actorId}")]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(
        OperationId = "DeleteActorByActorId",
        Summary = "Удалить актера",
        Description = "Удалить актера (только для администраторов)")]
    [SwaggerResponse(204, "Актер успешно удален")]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> DeleteActorByActorId([Required][FromRoute] int actorId)
    {
        try
        {
            var result = await _actorService.DeleteActorAsync(actorId);
            if (!result)
            {
                return NotFound(new ErrorDto("NotFound", "Actor not found"));
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

    //[HttpGet("{actorId}/roles")]
    //[Authorize]
    //[SwaggerOperation(
    //    OperationId = "GetRolesByActorId",
    //    Summary = "Получить все роли актера",
    //    Description = "Получить список всех ролей, которые исполняет конкретный актер")]
    //[SwaggerResponse(200, "Успешное получение ролей актера", typeof(RoleListDto))]
    //[SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    //[SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    //public async Task<IActionResult> GetRolesByActorId([Required][FromRoute] int actorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    //{
    //    try
    //    {
    //        // This would require additional service method to get actor roles with pagination
    //        // For now, returning not implemented
    //        return StatusCode(501, new ErrorDto("NotImplemented", "This endpoint is not yet implemented"));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Internal Server Error");
    //        return StatusCode(500, new ErrorDto("InternalServerError", "Internal server error"));
    //    }
    //}
}