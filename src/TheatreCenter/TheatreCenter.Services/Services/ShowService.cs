using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TheatreCenter.Services.Services
{
    public class ShowService : IShowService
    {
        private readonly IShowRepository _showRepository;
        private readonly IMusicalRepository _musicalRepository;

        public ShowService(IShowRepository showRepository, IMusicalRepository musicalRepository)
        {
            _showRepository = showRepository;
            _musicalRepository = musicalRepository;
        }

        public async Task<Show> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid show ID");

            var show = await _showRepository.GetByIdAsync(id);
            if (show == null)
                throw new KeyNotFoundException("Show not found");

            return show;
        }

        public async Task<IEnumerable<Show>> GetAllAsync()
        {
            return await _showRepository.GetAllAsync();
        }

        public async Task<Show> CreateAsync(Show show)
        {
            if (show == null)
                throw new ArgumentNullException(nameof(show));

            if (show.Date < DateTime.Now)
                throw new ArgumentException("Show date cannot be in the past");

            var musical = await _musicalRepository.GetByIdAsync(show.MusicalId);
            if (musical == null)
                throw new ArgumentException("Musical not found");

            await _showRepository.AddAsync(show);
            await _showRepository.SaveChangesAsync();
            return show;
        }

        public async Task<Show> UpdateAsync(Show show)
        {
            if (show == null)
                throw new ArgumentNullException(nameof(show));

            var existingShow = await _showRepository.GetByIdAsync(show.Id);
            if (existingShow == null)
                throw new KeyNotFoundException("Show not found");

            if (show.Date < DateTime.Now)
                throw new ArgumentException("Show date cannot be in the past");

            var musical = await _musicalRepository.GetByIdAsync(show.MusicalId);
            if (musical == null)
                throw new ArgumentException("Musical not found");

            existingShow.Date = show.Date;
            existingShow.MusicalId = show.MusicalId;

            await _showRepository.UpdateAsync(existingShow);
            await _showRepository.SaveChangesAsync();
            return existingShow;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var show = await _showRepository.GetByIdAsync(id);
            if (show == null)
                return false;

            if (show.Date < DateTime.Now)
                throw new InvalidOperationException("Cannot delete past shows");

            await _showRepository.RemoveAsync(show);
            await _showRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Show>> GetByMusicalIdAsync(int musicalId)
        {
            if (musicalId <= 0)
                throw new ArgumentException("Invalid musical ID");

            return await _showRepository.GetByMusicalIdAsync(musicalId);
        }

        public async Task<IEnumerable<Show>> GetUpcomingShowsAsync()
        {
            return await _showRepository.GetUpcomingShowsAsync();
        }
    }
}