using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TheatreCenter.Domain.Models;
using TheatreCenter.DTOs.CastMember;
using TheatreCenter.Services.Interfaces.Services;

namespace TheatreCenter.Backend.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CastMembersController : ControllerBase
    {
        private readonly ICastMemberService _castMemberService;

        public CastMembersController(ICastMemberService castMemberService)
        {
            _castMemberService = castMemberService;
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            OperationId = "GetCastMemberById",
            Summary = "Get cast member by ID",
            Description = "Returns a single cast member with the specified ID")]
        [SwaggerResponse(200, "Cast member found", typeof(CastMemberDTO))]
        [SwaggerResponse(404, "Cast member not found")]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var castMember = await _castMemberService.GetByIdAsync(id);
                var castMemberDto = new CastMemberDTO(
                    castMember.Id,
                    castMember.ShowId,
                    castMember.RoleId,
                    castMember.ActorId,
                    castMember.Comment
                );
                return Ok(castMemberDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [SwaggerOperation(
            OperationId = "GetAllCastMembers",
            Summary = "Get all cast members",
            Description = "Returns a list of all cast members")]
        [SwaggerResponse(200, "List of cast members", typeof(IEnumerable<CastMemberDTO>))]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var castMembers = await _castMemberService.GetAllAsync();
            var castMemberDtos = castMembers.Select(cm => new CastMemberDTO(
                cm.Id,
                cm.ShowId,
                cm.RoleId,
                cm.ActorId,
                cm.Comment
            ));
            return Ok(castMemberDtos);
        }

        [HttpPost]
        [SwaggerOperation(
            OperationId = "CreateCastMember",
            Summary = "Create new cast member",
            Description = "Creates a new cast member with the provided data")]
        [SwaggerResponse(201, "Cast member created", typeof(CastMemberDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateCastMemberDTO createDto)
        {
            try
            {
                var castMember = new CastMember(
                    0,
                    createDto.ShowId,
                    createDto.RoleId,
                    createDto.ActorId,
                    createDto.Comment
                );

                var createdCastMember = await _castMemberService.CreateAsync(castMember);
                var castMemberDto = new CastMemberDTO(
                    createdCastMember.Id,
                    createdCastMember.ShowId,
                    createdCastMember.RoleId,
                    createdCastMember.ActorId,
                    createdCastMember.Comment
                );

                return CreatedAtAction(nameof(GetById), new { id = castMemberDto.Id }, castMemberDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            OperationId = "UpdateCastMember",
            Summary = "Update cast member",
            Description = "Updates an existing cast member with the provided data")]
        [SwaggerResponse(200, "Cast member updated", typeof(CastMemberDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(404, "Cast member not found")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCastMemberDTO updateDto)
        {
            try
            {
                var existingCastMember = await _castMemberService.GetByIdAsync(id);

                existingCastMember.RoleId = updateDto.RoleId;
                existingCastMember.ActorId = updateDto.ActorId;

                var updatedCastMember = await _castMemberService.UpdateAsync(existingCastMember);
                var castMemberDto = new CastMemberDTO(
                    updatedCastMember.Id,
                    updatedCastMember.ShowId,
                    updatedCastMember.RoleId,
                    updatedCastMember.ActorId,
                    updatedCastMember.Comment
                );

                return Ok(castMemberDto);
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
            OperationId = "DeleteCastMember",
            Summary = "Delete cast member",
            Description = "Deletes a cast member with the specified ID")]
        [SwaggerResponse(204, "Cast member deleted")]
        [SwaggerResponse(400, "Cannot delete cast member")]
        [SwaggerResponse(404, "Cast member not found")]
        [SwaggerResponse(500, "Server error")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _castMemberService.DeleteAsync(id);
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

        [HttpGet("show/{showId}")]
        [SwaggerOperation(
            OperationId = "GetCastMembersByShow",
            Summary = "Get cast members by show",
            Description = "Returns cast members for a specific show")]
        [SwaggerResponse(200, "List of cast members", typeof(IEnumerable<CastMemberDTO>))]
        [SwaggerResponse(500, "Server error")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByShow(int showId)
        {
            try
            {
                var castMembers = await _castMemberService.GetByShowIdAsync(showId);
                var castMemberDtos = castMembers.Select(cm => new CastMemberDTO(
                    cm.Id,
                    cm.ShowId,
                    cm.RoleId,
                    cm.ActorId,
                    cm.Comment
                ));
                return Ok(castMemberDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}