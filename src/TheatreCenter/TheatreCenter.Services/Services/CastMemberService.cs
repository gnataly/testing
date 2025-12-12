using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using Microsoft.Extensions.Logging;

namespace TheatreCenter.Services.Services
{
    public class CastMemberService : ICastMemberService
    {
        private readonly ICastMemberRepository _castMemberRepository;
        private readonly IShowRepository _showRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IActorRepository _actorRepository;
        private readonly ILogger<CastMemberService> _logger;

        public CastMemberService(
            ICastMemberRepository castMemberRepository,
            IShowRepository showRepository,
            IRoleRepository roleRepository,
            IActorRepository actorRepository,
            ILogger<CastMemberService> logger)
        {
            _castMemberRepository = castMemberRepository;
            _showRepository = showRepository;
            _roleRepository = roleRepository;
            _actorRepository = actorRepository;
            _logger = logger;
        }

        public async Task<CastMember> GetByIdAsync(int id)
        {
            _logger.LogInformation("Attempting to get cast member with ID: {CastMemberId}", id);

            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid cast member ID provided: {CastMemberId}", id);
                    throw new ArgumentException("Invalid cast member ID");
                }

                var castMember = await _castMemberRepository.GetByIdAsync(id);
                if (castMember == null)
                {
                    _logger.LogWarning("Cast member with ID {CastMemberId} not found", id);
                    throw new KeyNotFoundException("Cast member not found");
                }

                _logger.LogInformation("Successfully retrieved cast member with ID: {CastMemberId}", id);
                return castMember;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting cast member with ID: {CastMemberId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<CastMember>> GetAllAsync(CastMemberFilter filter)
        {
            _logger.LogInformation("Starting to retrieve all cast members with filter");

            try
            {
                var result = await _castMemberRepository.GetAllAsync(filter);
                _logger.LogInformation("Retrieved {CastMemberCount} cast members", result.Count());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all cast members");
                throw;
            }
        }

        public async Task<CastMember> CreateAsync(CastMember castMember)
        {
            _logger.LogInformation("Attempting to create new cast member");

            try
            {
                if (castMember == null)
                {
                    _logger.LogError("Attempted to create null cast member");
                    throw new ArgumentNullException(nameof(castMember));
                }

                var show = await _showRepository.GetByIdAsync(castMember.ShowId);
                if (show == null)
                {
                    _logger.LogError("Show with ID {ShowId} not found for cast member creation", castMember.ShowId);
                    throw new ArgumentException("Show not found");
                }

                var role = await _roleRepository.GetByIdAsync(castMember.RoleId);
                if (role == null)
                {
                    _logger.LogError("Role with ID {RoleId} not found for cast member creation", castMember.RoleId);
                    throw new ArgumentException("Role not found");
                }

                var actor = await _actorRepository.GetByIdAsync(castMember.ActorId);
                if (actor == null)
                {
                    _logger.LogError("Actor with ID {ActorId} not found for cast member creation", castMember.ActorId);
                    throw new ArgumentException("Actor not found");
                }

                // Check if this role is already assigned to another actor in this show
                var existingAssignment = await _castMemberRepository.GetByShowAndRoleAsync(castMember.ShowId, castMember.RoleId);
                if (existingAssignment != null)
                {
                    _logger.LogError("Role {RoleId} is already assigned to another actor for show {ShowId}",
                        castMember.RoleId, castMember.ShowId);
                    throw new InvalidOperationException("This role is already assigned to another actor for this show");
                }

                await _castMemberRepository.AddAsync(castMember);
                await _castMemberRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully created new cast member with ID: {CastMemberId}", castMember.Id);
                return castMember;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating cast member");
                throw;
            }
        }

        public async Task<CastMember> UpdateAsync(CastMember castMember)
        {
            _logger.LogInformation("Attempting to update cast member with ID: {CastMemberId}", castMember?.Id);

            try
            {
                if (castMember == null)
                {
                    _logger.LogError("Attempted to update null cast member");
                    throw new ArgumentNullException(nameof(castMember));
                }

                var existingCastMember = await _castMemberRepository.GetByIdAsync(castMember.Id);
                if (existingCastMember == null)
                {
                    _logger.LogWarning("Cast member with ID {CastMemberId} not found for update", castMember.Id);
                    throw new KeyNotFoundException("Cast member not found");
                }

                var show = await _showRepository.GetByIdAsync(castMember.ShowId);
                if (show == null)
                {
                    _logger.LogError("Show with ID {ShowId} not found for cast member update", castMember.ShowId);
                    throw new ArgumentException("Show not found");
                }

                var role = await _roleRepository.GetByIdAsync(castMember.RoleId);
                if (role == null)
                {
                    _logger.LogError("Role with ID {RoleId} not found for cast member update", castMember.RoleId);
                    throw new ArgumentException("Role not found");
                }

                var actor = await _actorRepository.GetByIdAsync(castMember.ActorId);
                if (actor == null)
                {
                    _logger.LogError("Actor with ID {ActorId} not found for cast member update", castMember.ActorId);
                    throw new ArgumentException("Actor not found");
                }

                var existingAssignment = await _castMemberRepository.GetByShowAndRoleAsync(castMember.ShowId, castMember.RoleId);
                if (existingAssignment != null && existingAssignment.Id != castMember.Id)
                {
                    _logger.LogError("Role {RoleId} is already assigned to another actor for show {ShowId}",
                        castMember.RoleId, castMember.ShowId);
                    throw new InvalidOperationException("This role is already assigned to another actor for this show");
                }

                existingCastMember.ShowId = castMember.ShowId;
                existingCastMember.RoleId = castMember.RoleId;
                existingCastMember.ActorId = castMember.ActorId;
                existingCastMember.Comment = castMember.Comment;

                await _castMemberRepository.UpdateAsync(existingCastMember);
                await _castMemberRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully updated cast member with ID: {CastMemberId}", castMember.Id);
                return existingCastMember;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating cast member with ID: {CastMemberId}", castMember?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Attempting to delete cast member with ID: {CastMemberId}", id);

            try
            {
                var castMember = await _castMemberRepository.GetByIdAsync(id);
                if (castMember == null)
                {
                    _logger.LogWarning("Cast member with ID {CastMemberId} not found for deletion", id);
                    return false;
                }

                await _castMemberRepository.RemoveAsync(castMember);
                await _castMemberRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted cast member with ID: {CastMemberId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting cast member with ID: {CastMemberId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<CastMember>> GetByShowIdAsync(int showId)
        {
            _logger.LogInformation("Getting cast members by show ID: {ShowId}", showId);

            try
            {
                if (showId <= 0)
                {
                    _logger.LogWarning("Invalid show ID provided: {ShowId}", showId);
                    throw new ArgumentException("Invalid show ID");
                }

                var castMembers = await _castMemberRepository.GetByShowIdAsync(showId);
                _logger.LogInformation("Found {CastMemberCount} cast members for show {ShowId}",
                    castMembers.Count(), showId);
                return castMembers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting cast members by show ID: {ShowId}", showId);
                throw;
            }
        }

        public async Task<IEnumerable<CastMember>> GetByActorIdAsync(int actorId)
        {
            _logger.LogInformation("Getting cast members by actor ID: {ActorId}", actorId);

            try
            {
                if (actorId <= 0)
                {
                    _logger.LogWarning("Invalid actor ID provided: {ActorId}", actorId);
                    throw new ArgumentException("Invalid actor ID");
                }

                var castMembers = await _castMemberRepository.GetByActorIdAsync(actorId);
                _logger.LogInformation("Found {CastMemberCount} cast members for actor {ActorId}",
                    castMembers.Count(), actorId);
                return castMembers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting cast members by actor ID: {ActorId}", actorId);
                throw;
            }
        }
    }
}
