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
[Route("/api/v1/roles")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;

    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetAllRoles",
        Summary = "Получить все роли с фильтрацией",
        Description = "Получить пагинированный список ролей с возможностью фильтрации по различным параметрам")]
    [SwaggerResponse(200, "Успешное получение списка ролей", typeof(RoleListDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
    public async Task<IActionResult> GetAllRoles(
        [FromQuery] RoleType? roleType,
        [FromQuery] int? musicalId,
        [FromQuery] int? actorId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] SortType sort = SortType.id_asc)
    {
        try
        {
            var filter = new RoleFilter
            {
                Page = page,
                PageSize = pageSize,
                Search = search,
                RoleType = roleType.HasValue ? roleType.Value : null,
                MusicalId = musicalId,
                ActorId = actorId,
                Sort = sort.ToString()
            };

            var result = await _roleService.GetAllAsync(filter);

            var totalCount = result.Count();
            var items = result
                .OrderBy(a => a.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var roleDtos = items.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                MusicalId = r.MusicalId,
                RoleType = r.RoleType
            }).ToList();

            var response = new RoleListDto
            {
                Items = roleDtos,
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
        OperationId = "CreateRole",
        Summary = "Создать новую роль",
        Description = "Создать новую роль (только для администраторов)")]
    [SwaggerResponse(201, "Роль успешно создана", typeof(RoleDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
    public async Task<IActionResult> CreateRole([Required][FromBody] CreateRoleRequestDto createRoleRequestDto)
    {
        try
        {
            var role = new Role(
                id: 0,
                name: createRoleRequestDto.Name,
                musicalId: createRoleRequestDto.MusicalId,
                roleType: createRoleRequestDto.RoleType
            );

            var createdRole = await _roleService.CreateAsync(role);

            var roleDto = new RoleDto
            {
                Id = createdRole.Id,
                Name = createdRole.Name,
                MusicalId = createdRole.MusicalId,
                RoleType = createdRole.RoleType
            };

            return CreatedAtAction(nameof(GetRoleByRoleId), new { roleId = roleDto.Id }, roleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal Server Error");
            return BadRequest(new ErrorDto("ValidationError", ex.Message));
        }
    }

    [HttpGet("{roleId}")]
    [SwaggerOperation(
        OperationId = "GetRoleByRoleId",
        Summary = "Получить роль по ID",
        Description = "Получить информацию о роли по ее ID")]
    [SwaggerResponse(200, "Успешное получение роли", typeof(RoleDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> GetRoleByRoleId([Required][FromRoute] int roleId)
    {
        try
        {
            var role = await _roleService.GetByIdAsync(roleId);

            var roleDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                MusicalId = role.MusicalId,
                RoleType = role.RoleType
            };

            return Ok(roleDto);
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

    [HttpPut("{roleId}")]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(
        OperationId = "UpdateRoleByRoleId",
        Summary = "Обновить роль",
        Description = "Обновить информацию о роли (только для администраторов)")]
    [SwaggerResponse(200, "Роль успешно обновлена", typeof(RoleDto))]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(400, "Ошибка валидации", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> UpdateRoleByRoleId([Required][FromRoute] int roleId, [Required][FromBody] UpdateRoleRequestDto updateRoleRequestDto)
    {
        try
        {
            var existingRole = await _roleService.GetByIdAsync(roleId);

            var role = new Role(
                id: roleId,
                name: updateRoleRequestDto.Name ?? existingRole.Name,
                musicalId: existingRole.MusicalId,
                roleType: updateRoleRequestDto.RoleType
            );

            var updatedRole = await _roleService.UpdateAsync(role);

            var roleDto = new RoleDto
            {
                Id = updatedRole.Id,
                Name = updatedRole.Name,
                MusicalId = updatedRole.MusicalId,
                RoleType = updatedRole.RoleType
            };

            return Ok(roleDto);
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

    [HttpDelete("{roleId}")]
    [Authorize(Policy = "AdminOnly")]
    [SwaggerOperation(
        OperationId = "DeleteRoleByRoleId",
        Summary = "Удалить роль",
        Description = "Удалить роль (только для администраторов)")]
    [SwaggerResponse(204, "Роль успешно удалена")]
    [SwaggerResponse(401, "Не авторизован", typeof(ErrorDto))]
    [SwaggerResponse(403, "Запрещено", typeof(ErrorDto))]
    [SwaggerResponse(404, "Ресурс не найден", typeof(ErrorDto))]
    public async Task<IActionResult> DeleteRoleByRoleId([Required][FromRoute] int roleId)
    {
        try
        {
            var result = await _roleService.DeleteAsync(roleId);
            if (!result)
            {
                return NotFound(new ErrorDto("NotFound", "Role not found"));
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
