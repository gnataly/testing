using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TheatreCenter.DTOs;
using TheatreCenter.Services.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using TheatreCenter.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace TheatreCenter.Backend.WebAPI.Controllers
{
    [ApiController]
    [Route("/api/v1/cast_members")]
    public class CastMembersController : ControllerBase
    {
        private readonly ICastMemberService _castMemberService;
        private readonly ILogger<CastMembersController> _logger;

        public CastMembersController(ICastMemberService castMemberService, ILogger<CastMembersController> logger)
        {
            _castMemberService = castMemberService;
            _logger = logger;
        }

        [HttpGet]
        [SwaggerOperation(
            OperationId = "GetAllCastMembers",
            Summary = "Получить всех участников каста с фильтрацией",
            Description = "Получить пагинированный список участников каста с возможностью фильтрации по различным параметрам")]
        [SwaggerResponse(200, "Успешное получение списка участников каста", typeof(CastMemberListDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
        public async Task<IActionResult> GetAllCastMembers(
            [FromQuery] int? showId,
            [FromQuery] int? roleId,
            [FromQuery] int? actorId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var filter = new CastMemberFilter
                {
                    Page = page,
                    PageSize = pageSize,
                    ShowId = showId,
                    RoleId = roleId,
                    ActorId = actorId
                };

                var result = await _castMemberService.GetAllAsync(filter);

                var totalCount = result.Count();
                var items = result
                    .OrderBy(a => a.Id)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var castMemberDtos = items.Select(cm => new CastMemberDto
                {
                    Id = cm.Id,
                    ShowId = cm.ShowId,
                    RoleId = cm.RoleId,
                    ActorId = cm.ActorId,
                    Comment = cm.Comment
                }).ToList();

                var response = new CastMemberListDto
                {
                    Items = castMemberDtos,
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
            OperationId = "CreateCastMember",
            Summary = "Создать нового участника каста",
            Description = "Создать нового участника каста (только для администраторов)")]
        [SwaggerResponse(201, "Участник каста успешно создан", typeof(CastMemberDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
        [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
        public async Task<IActionResult> CreateCastMember([Required][FromBody] CreateCastMemberRequestDto createCastMemberRequestDto)
        {
            try
            {
                var castMember = new CastMember(
                    id: 0,
                    showId: createCastMemberRequestDto.ShowId,
                    roleId: createCastMemberRequestDto.RoleId,
                    actorId: createCastMemberRequestDto.ActorId,
                    comment: createCastMemberRequestDto.Comment
                );

                var createdCastMember = await _castMemberService.CreateAsync(castMember);

                var castMemberDto = new CastMemberDto
                {
                    Id = createdCastMember.Id,
                    ShowId = createdCastMember.ShowId,
                    RoleId = createdCastMember.RoleId,
                    ActorId = createdCastMember.ActorId,
                    Comment = createdCastMember.Comment
                };

                return CreatedAtAction(nameof(GetCastMemberByCastMemberId), new { castMemberId = castMemberDto.Id }, castMemberDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                return BadRequest(new ErrorDto("ValidationError", ex.Message));
            }
        }

        [HttpGet("{castMemberId}")]
        [SwaggerOperation(
            OperationId = "GetCastMemberByCastMemberId",
            Summary = "Получить участника каста по ID",
            Description = "Получить информацию об участнике каста по его ID")]
        [SwaggerResponse(200, "Успешное получение участника каста", typeof(CastMemberDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
        public async Task<IActionResult> GetCastMemberByCastMemberId([Required][FromRoute] int castMemberId)
        {
            try
            {
                var castMember = await _castMemberService.GetByIdAsync(castMemberId);

                var castMemberDto = new CastMemberDto
                {
                    Id = castMember.Id,
                    ShowId = castMember.ShowId,
                    RoleId = castMember.RoleId,
                    ActorId = castMember.ActorId,
                    Comment = castMember.Comment
                };

                return Ok(castMemberDto);
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

        [HttpPut("{castMemberId}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            OperationId = "UpdateCastMemberByCastMemberId",
            Summary = "Обновить участника каста",
            Description = "Обновить информацию об участнике каста (только для администраторов)")]
        [SwaggerResponse(200, "Участник каста успешно обновлен", typeof(CastMemberDto))]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
        [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
        [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
        public async Task<IActionResult> UpdateCastMemberByCastMemberId([Required][FromRoute] int castMemberId, [Required][FromBody] UpdateCastMemberRequestDto updateCastMemberRequestDto)
        {
            try
            {
                var existingCastMember = await _castMemberService.GetByIdAsync(castMemberId);

                var castMember = new CastMember(
                    id: castMemberId,
                    showId: existingCastMember.ShowId,
                    roleId: updateCastMemberRequestDto.RoleId,
                    actorId: updateCastMemberRequestDto.ActorId,
                    comment: updateCastMemberRequestDto.Comment
                );

                var updatedCastMember = await _castMemberService.UpdateAsync(castMember);

                var castMemberDto = new CastMemberDto
                {
                    Id = updatedCastMember.Id,
                    ShowId = updatedCastMember.ShowId,
                    RoleId = updatedCastMember.RoleId,
                    ActorId = updatedCastMember.ActorId,
                    Comment = updatedCastMember.Comment
                };

                return Ok(castMemberDto);
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

        [HttpDelete("{castMemberId}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            OperationId = "DeleteCastMemberByCastMemberId",
            Summary = "Удалить участника каста",
            Description = "Удалить участника каста (только для администраторов)")]
        [SwaggerResponse(204, "Участник каста успешно удален")]
        [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
        [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
        [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
        public async Task<IActionResult> DeleteCastMemberByCastMemberId([Required][FromRoute] int castMemberId)
        {
            try
            {
                var result = await _castMemberService.DeleteAsync(castMemberId);
                if (!result)
                {
                    return NotFound(new ErrorDto("NotFound", "Cast member not found"));
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