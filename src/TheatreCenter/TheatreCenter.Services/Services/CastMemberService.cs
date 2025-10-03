using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TheatreCenter.Services.Services
{
    public class CastMemberService : ICastMemberService
    {
        private readonly ICastMemberRepository _castMemberRepository;
        private readonly IShowRepository _showRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IActorRepository _actorRepository;

        public CastMemberService(
            ICastMemberRepository castMemberRepository,
            IShowRepository showRepository,
            IRoleRepository roleRepository,
            IActorRepository actorRepository)
        {
            _castMemberRepository = castMemberRepository;
            _showRepository = showRepository;
            _roleRepository = roleRepository;
            _actorRepository = actorRepository;
        }

        public async Task<CastMember> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid cast member ID");

            var castMember = await _castMemberRepository.GetByIdAsync(id);
            if (castMember == null)
                throw new KeyNotFoundException("Cast member not found");

            return castMember;
        }

        public async Task<IEnumerable<CastMember>> GetAllAsync()
        {
            return await _castMemberRepository.GetAllAsync();
        }

        public async Task<CastMember> CreateAsync(CastMember castMember)
        {
            if (castMember == null)
                throw new ArgumentNullException(nameof(castMember));

            var show = await _showRepository.GetByIdAsync(castMember.ShowId);
            if (show == null)
                throw new ArgumentException("Show not found");

            var role = await _roleRepository.GetByIdAsync(castMember.RoleId);
            if (role == null)
                throw new ArgumentException("Role not found");

            var actor = await _actorRepository.GetByIdAsync(castMember.ActorId);
            if (actor == null)
                throw new ArgumentException("Actor not found");

            // Check if this role is already assigned to another actor in this show
            var existingAssignment = await _castMemberRepository.GetByShowAndRoleAsync(castMember.ShowId, castMember.RoleId);
            if (existingAssignment != null)
                throw new InvalidOperationException("This role is already assigned to another actor for this show");

            await _castMemberRepository.AddAsync(castMember);
            await _castMemberRepository.SaveChangesAsync();
            return castMember;
        }

        public async Task<CastMember> UpdateAsync(CastMember castMember)
        {
            if (castMember == null)
                throw new ArgumentNullException(nameof(castMember));

            var existingCastMember = await _castMemberRepository.GetByIdAsync(castMember.Id);
            if (existingCastMember == null)
                throw new KeyNotFoundException("Cast member not found");

            var show = await _showRepository.GetByIdAsync(castMember.ShowId);
            if (show == null)
                throw new ArgumentException("Show not found");

            var role = await _roleRepository.GetByIdAsync(castMember.RoleId);
            if (role == null)
                throw new ArgumentException("Role not found");

            var actor = await _actorRepository.GetByIdAsync(castMember.ActorId);
            if (actor == null)
                throw new ArgumentException("Actor not found");

            // Check if this role is already assigned to another actor in this show
            var existingAssignment = await _castMemberRepository.GetByShowAndRoleAsync(castMember.ShowId, castMember.RoleId);
            if (existingAssignment != null && existingAssignment.Id != castMember.Id)
                throw new InvalidOperationException("This role is already assigned to another actor for this show");

            existingCastMember.ShowId = castMember.ShowId;
            existingCastMember.RoleId = castMember.RoleId;
            existingCastMember.ActorId = castMember.ActorId;

            await _castMemberRepository.UpdateAsync(existingCastMember);
            await _castMemberRepository.SaveChangesAsync();
            return existingCastMember;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var castMember = await _castMemberRepository.GetByIdAsync(id);
            if (castMember == null)
                return false;

            await _castMemberRepository.RemoveAsync(castMember);
            await _castMemberRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CastMember>> GetByShowIdAsync(int showId)
        {
            if (showId <= 0)
                throw new ArgumentException("Invalid show ID");

            return await _castMemberRepository.GetByShowIdAsync(showId);
        }

        public async Task<IEnumerable<CastMember>> GetByActorIdAsync(int actorId)
        {
            if (actorId <= 0)
                throw new ArgumentException("Invalid actor ID");

            return await _castMemberRepository.GetByActorIdAsync(actorId);
        }
    }
}