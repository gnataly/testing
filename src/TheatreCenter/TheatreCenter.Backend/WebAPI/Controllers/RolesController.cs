using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TheatreCenter.Domain.Models;
using TheatreCenter.DTOs.Role;
using TheatreCenter.Services.Interfaces.Services;

namespace TheatreCenter.Backend.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            OperationId = "GetRoleById",
            Summary = "Get role by ID",
            Description = "Returns a single role with the specified ID")]
        [SwaggerResponse(200, "Role found", typeof(RoleDTO))]
        [SwaggerResponse(404, "Role not found")]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var role = await _roleService.GetByIdAsync(id);
                var roleDto = new RoleDTO(
                    role.Id,
                    role.Name,
                    role.MusicalId,
                    role.RoleType
                );
                return Ok(roleDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [SwaggerOperation(
            OperationId = "GetAllRoles",
            Summary = "Get all roles",
            Description = "Returns a list of all roles")]
        [SwaggerResponse(200, "List of roles", typeof(IEnumerable<RoleDTO>))]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllAsync();
            var roleDtos = roles.Select(r => new RoleDTO(
                r.Id,
                r.Name,
                r.MusicalId,
                r.RoleType
            ));
            return Ok(roleDtos);
        }

        [HttpGet("musical/{musicalId}")]
        [SwaggerOperation(
            OperationId = "GetRolesByMusical",
            Summary = "Get roles by musical",
            Description = "Returns roles for a specific musical")]
        [SwaggerResponse(200, "List of roles", typeof(IEnumerable<RoleDTO>))]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByMusical(int musicalId)
        {
            try
            {
                var roles = await _roleService.GetByMusicalIdAsync(musicalId);
                var roleDtos = roles.Select(r => new RoleDTO(
                    r.Id,
                    r.Name,
                    r.MusicalId,
                    r.RoleType
                ));
                return Ok(roleDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [SwaggerOperation(
            OperationId = "CreateRole",
            Summary = "Create new role",
            Description = "Creates a new role with the provided data")]
        [SwaggerResponse(201, "Role created", typeof(RoleDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateRoleDTO createDto)
        {
            try
            {
                var role = new Role(
                    0,
                    createDto.Name,
                    createDto.MusicalId,
                    createDto.RoleType
                );

                var createdRole = await _roleService.CreateAsync(role);
                var roleDto = new RoleDTO(
                    createdRole.Id,
                    createdRole.Name,
                    createdRole.MusicalId,
                    createdRole.RoleType
                );

                return CreatedAtAction(nameof(GetById), new { id = roleDto.Id }, roleDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            OperationId = "UpdateRole",
            Summary = "Update role",
            Description = "Updates an existing role with the provided data")]
        [SwaggerResponse(200, "Role updated", typeof(RoleDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Role not found")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleDTO updateDto)
        {
            try
            {
                var existingRole = await _roleService.GetByIdAsync(id);

                existingRole.RoleType = updateDto.RoleType;

                var updatedRole = await _roleService.UpdateAsync(existingRole);
                var roleDto = new RoleDTO(
                    updatedRole.Id,
                    updatedRole.Name,
                    updatedRole.MusicalId,
                    updatedRole.RoleType
                );

                return Ok(roleDto);
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
            OperationId = "DeleteRole",
            Summary = "Delete role",
            Description = "Deletes a role with the specified ID")]
        [SwaggerResponse(204, "Role deleted")]
        [SwaggerResponse(400, "Cannot delete role")]
        [SwaggerResponse(404, "Role not found")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _roleService.DeleteAsync(id);
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