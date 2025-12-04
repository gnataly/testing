using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace TheatreCenter.Services.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMusicalRepository _musicalRepository;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            IRoleRepository roleRepository,
            IMusicalRepository musicalRepository,
            ILogger<RoleService> logger)
        {
            _roleRepository = roleRepository;
            _musicalRepository = musicalRepository;
            _logger = logger;
        }

        public async Task<Role> GetByIdAsync(int id)
        {
            _logger.LogInformation("Attempting to get role with ID: {RoleId}", id);

            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid role ID");

                var role = await _roleRepository.GetByIdAsync(id);
                if (role == null)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found", id);
                    throw new KeyNotFoundException("Role not found");
                }

                _logger.LogInformation("Successfully retrieved role with ID: {RoleId}", id);
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting role with ID: {RoleId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Role>> GetAllAsync(RoleFilter filter)
        {
            _logger.LogInformation("Starting to retrieve all roles with filter");

            try
            {
                var result = await _roleRepository.GetAllAsync(filter);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all roles");
                throw;
            }
        }

        public async Task<Role> CreateAsync(Role role)
        {
            _logger.LogInformation("Attempting to create new role");

            try
            {
                if (role == null)
                {
                    _logger.LogError("Attempted to create null role");
                    throw new ArgumentNullException(nameof(role));
                }

                if (string.IsNullOrWhiteSpace(role.Name))
                    throw new ArgumentException("Role name cannot be empty");

                var musical = await _musicalRepository.GetByIdAsync(role.MusicalId);
                if (musical == null)
                    throw new ArgumentException("Musical not found");

                await _roleRepository.AddAsync(role);
                await _roleRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully created new role with ID: {RoleId}", role.Id);
                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating role");
                throw;
            }
        }

        public async Task<Role> UpdateAsync(Role role)
        {
            _logger.LogInformation("Attempting to update role with ID: {RoleId}", role?.Id);

            try
            {
                if (role == null)
                {
                    _logger.LogError("Attempted to update null role");
                    throw new ArgumentNullException(nameof(role));
                }

                var existingRole = await _roleRepository.GetByIdAsync(role.Id);
                if (existingRole == null)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found for update", role.Id);
                    throw new KeyNotFoundException("Role not found");
                }

                if (string.IsNullOrWhiteSpace(role.Name))
                    throw new ArgumentException("Role name cannot be empty");

                var musical = await _musicalRepository.GetByIdAsync(role.MusicalId);
                if (musical == null)
                    throw new ArgumentException("Musical not found");

                existingRole.Name = role.Name;
                existingRole.RoleType = role.RoleType;
                existingRole.MusicalId = role.MusicalId;

                await _roleRepository.UpdateAsync(existingRole);
                await _roleRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully updated role with ID: {RoleId}", role.Id);
                return existingRole;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating role with ID: {RoleId}", role?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Attempting to delete role with ID: {RoleId}", id);

            try
            {
                var role = await _roleRepository.GetByIdAsync(id);
                if (role == null)
                {
                    _logger.LogWarning("Role with ID {RoleId} not found for deletion", id);
                    return false;
                }

                if (role.ActorRoles.Any())
                {
                    _logger.LogError("Cannot delete role with ID {RoleId} - has assigned actors", id);
                    throw new InvalidOperationException("Cannot delete role with assigned actors");
                }

                await _roleRepository.RemoveAsync(role);
                await _roleRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted role with ID: {RoleId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting role with ID: {RoleId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Role>> GetByMusicalIdAsync(int musicalId)
        {
            _logger.LogInformation("Getting roles by musical ID: {MusicalId}", musicalId);

            try
            {
                if (musicalId <= 0)
                    throw new ArgumentException("Invalid musical ID");

                var roles = await _roleRepository.GetByMusicalIdAsync(musicalId);
                _logger.LogInformation("Found {RoleCount} roles for musical {MusicalId}",
                    roles.Count(), musicalId);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting roles by musical ID: {MusicalId}", musicalId);
                throw;
            }
        }

        public async Task<IEnumerable<Role>> GetByRoleTypeAsync(RoleType roleType)
        {
            _logger.LogInformation("Getting roles by role type: {RoleType}", roleType);

            try
            {
                var roles = await _roleRepository.GetByRoleTypeAsync(roleType);
                _logger.LogInformation("Found {RoleCount} roles with type {RoleType}",
                    roles.Count(), roleType);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting roles by role type: {RoleType}", roleType);
                throw;
            }
        }

        public async Task<IEnumerable<Role>> GetActorRolesAsync(int actorId)
        {
            _logger.LogInformation("Getting roles for actor ID: {ActorId}", actorId);

            try
            {
                if (actorId <= 0)
                    throw new ArgumentException("Invalid actor ID");

                var roles = await _roleRepository.GetActorRolesAsync(actorId);
                _logger.LogInformation("Found {RoleCount} roles for actor {ActorId}",
                    roles.Count(), actorId);
                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting roles for actor ID: {ActorId}", actorId);
                throw;
            }
        }
    }
}