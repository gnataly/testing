using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TheatreCenter.Services.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMusicalRepository _musicalRepository;

        public RoleService(IRoleRepository roleRepository, IMusicalRepository musicalRepository)
        {
            _roleRepository = roleRepository;
            _musicalRepository = musicalRepository;
        }

        public async Task<Role> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid role ID");

            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                throw new KeyNotFoundException("Role not found");

            return role;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _roleRepository.GetAllAsync();
        }

        public async Task<Role> CreateAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (string.IsNullOrWhiteSpace(role.Name))
                throw new ArgumentException("Role name cannot be empty");

            var musical = await _musicalRepository.GetByIdAsync(role.MusicalId);
            if (musical == null)
                throw new ArgumentException("Musical not found");

            await _roleRepository.AddAsync(role);
            await _roleRepository.SaveChangesAsync();
            return role;
        }

        public async Task<Role> UpdateAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var existingRole = await _roleRepository.GetByIdAsync(role.Id);
            if (existingRole == null)
                throw new KeyNotFoundException("Role not found");

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
            return existingRole;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                return false;

            if (role.ActorRoles.Any())
                throw new InvalidOperationException("Cannot delete role with assigned actors");

            await _roleRepository.RemoveAsync(role);
            await _roleRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Role>> GetByMusicalIdAsync(int musicalId)
        {
            if (musicalId <= 0)
                throw new ArgumentException("Invalid musical ID");

            return await _roleRepository.GetByMusicalIdAsync(musicalId);
        }

        public async Task<IEnumerable<Role>> GetByRoleTypeAsync(RoleType roleType)
        {
            return await _roleRepository.GetByRoleTypeAsync(roleType);
        }
    }
}