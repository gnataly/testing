using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Services.Services
{
    public class MusicalService : IMusicalService
    {
        private readonly IMusicalRepository _musicalRepository;
        private readonly ITheatreRepository _theatreRepository;

        public MusicalService(IMusicalRepository musicalRepository, ITheatreRepository theatreRepository)
        {
            _musicalRepository = musicalRepository;
            _theatreRepository = theatreRepository;
        }

        public async Task<Musical> GetMusicalByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid musical ID");

            var musical = await _musicalRepository.GetByIdAsync(id);
            if (musical == null)
                throw new KeyNotFoundException("Musical not found");

            return musical;
        }

        public async Task<IEnumerable<Musical>> GetAllMusicalsAsync()
        {
            return await _musicalRepository.GetAllAsync();
        }

        public async Task<Musical> CreateMusicalAsync(Musical musical)
        {
            if (musical == null)
                throw new ArgumentNullException(nameof(musical));

            if (string.IsNullOrWhiteSpace(musical.Title))
                throw new ArgumentException("Title cannot be empty");

            if (musical.Duration <= TimeSpan.Zero)
                throw new ArgumentException("Duration must be positive");

            var theatre = await _theatreRepository.GetByIdAsync(musical.TheatreId);
            if (theatre == null)
                throw new ArgumentException("Theatre not found");

            await _musicalRepository.AddAsync(musical);
            await _musicalRepository.SaveChangesAsync();
            return musical;
        }

        public async Task<Musical> UpdateMusicalAsync(Musical musical)
        {
            if (musical == null)
                throw new ArgumentNullException(nameof(musical));

            var existingMusical = await _musicalRepository.GetByIdAsync(musical.Id);
            if (existingMusical == null)
                throw new KeyNotFoundException("Musical not found");

            if (string.IsNullOrWhiteSpace(musical.Title))
                throw new ArgumentException("Title cannot be empty");

            var theatre = await _theatreRepository.GetByIdAsync(musical.TheatreId);
            if (theatre == null)
                throw new ArgumentException("Theatre not found");

            existingMusical.Title = musical.Title;
            existingMusical.Description = musical.Description;
            existingMusical.Duration = musical.Duration;
            existingMusical.AgeRestriction = musical.AgeRestriction;
            existingMusical.TheatreId = musical.TheatreId;

            await _musicalRepository.UpdateAsync(existingMusical);
            await _musicalRepository.SaveChangesAsync();
            return existingMusical;
        }

        public async Task<bool> DeleteMusicalAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid musical ID");

            var musical = await _musicalRepository.GetByIdAsync(id);
            if (musical == null)
                return false;

            if (musical.Shows.Any())
                throw new InvalidOperationException("Cannot delete musical with scheduled shows");

            await _musicalRepository.RemoveAsync(musical);
            await _musicalRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Musical>> GetMusicalsByTheatreAsync(int theatreId)
        {
            if (theatreId <= 0)
                throw new ArgumentException("Invalid theatre ID");

            return await _musicalRepository.GetByTheatreIdAsync(theatreId);
        }

        public async Task<IEnumerable<Musical>> GetMusicalsByAgeRestrictionAsync(AgeRestriction ageRestriction)
        {
            return await _musicalRepository.GetByAgeRestrictionAsync(ageRestriction);
        }
    }
}
