using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using Microsoft.Extensions.Logging;

namespace TheatreCenter.Services.Services
{
    public class ShowService : IShowService
    {
        private readonly IShowRepository _showRepository;
        private readonly IMusicalRepository _musicalRepository;
        private readonly ILogger<ShowService> _logger;

        public ShowService(
            IShowRepository showRepository,
            IMusicalRepository musicalRepository,
            ILogger<ShowService> logger)
        {
            _showRepository = showRepository;
            _musicalRepository = musicalRepository;
            _logger = logger;
        }

        public async Task<Show> GetByIdAsync(int id)
        {
            _logger.LogInformation("Attempting to get show with ID: {ShowId}", id);

            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid show ID provided: {ShowId}", id);
                    throw new ArgumentException("Invalid show ID");
                }

                var show = await _showRepository.GetByIdAsync(id);
                if (show == null)
                {
                    _logger.LogWarning("Show with ID {ShowId} not found", id);
                    throw new KeyNotFoundException("Show not found");
                }

                _logger.LogInformation("Successfully retrieved show with ID: {ShowId}", id);
                return show;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting show with ID: {ShowId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Show>> GetAllAsync(ShowFilter filter)
        {
            _logger.LogInformation("Starting to retrieve all shows with filter");

            try
            {
                var result = await _showRepository.GetAllAsync(filter);
                _logger.LogInformation("Retrieved {ShowCount} shows", result.Count());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all shows");
                throw;
            }
        }

        public async Task<Show> CreateAsync(Show show)
        {
            _logger.LogInformation("Attempting to create new show");

            try
            {
                if (show == null)
                {
                    _logger.LogError("Attempted to create null show");
                    throw new ArgumentNullException(nameof(show));
                }

                if (show.Date < DateTime.Now)
                {
                    _logger.LogError("Attempted to create show with past date: {ShowDate}", show.Date);
                    throw new ArgumentException("Show date cannot be in the past");
                }

                var musical = await _musicalRepository.GetByIdAsync(show.MusicalId);
                if (musical == null)
                {
                    _logger.LogError("Musical with ID {MusicalId} not found for show creation", show.MusicalId);
                    throw new ArgumentException("Musical not found");
                }

                await _showRepository.AddAsync(show);
                await _showRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully created new show with ID: {ShowId}", show.Id);
                return show;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating show");
                throw;
            }
        }

        public async Task<Show> UpdateAsync(Show show)
        {
            _logger.LogInformation("Attempting to update show with ID: {ShowId}", show?.Id);

            try
            {
                if (show == null)
                {
                    _logger.LogError("Attempted to update null show");
                    throw new ArgumentNullException(nameof(show));
                }

                var existingShow = await _showRepository.GetByIdAsync(show.Id);
                if (existingShow == null)
                {
                    _logger.LogWarning("Show with ID {ShowId} not found for update", show.Id);
                    throw new KeyNotFoundException("Show not found");
                }

                if (show.Date < DateTime.Now)
                {
                    _logger.LogError("Attempted to update show with past date: {ShowDate}", show.Date);
                    throw new ArgumentException("Show date cannot be in the past");
                }

                var musical = await _musicalRepository.GetByIdAsync(show.MusicalId);
                if (musical == null)
                {
                    _logger.LogError("Musical with ID {MusicalId} not found for show update", show.MusicalId);
                    throw new ArgumentException("Musical not found");
                }

                existingShow.Date = show.Date;
                existingShow.MusicalId = show.MusicalId;

                await _showRepository.UpdateAsync(existingShow);
                await _showRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully updated show with ID: {ShowId}", show.Id);
                return existingShow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating show with ID: {ShowId}", show?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Attempting to delete show with ID: {ShowId}", id);

            try
            {
                var show = await _showRepository.GetByIdAsync(id);
                if (show == null)
                {
                    _logger.LogWarning("Show with ID {ShowId} not found for deletion", id);
                    return false;
                }

                if (show.Date < DateTime.Now)
                {
                    _logger.LogError("Cannot delete show with ID {ShowId} - it's a past show", id);
                    throw new InvalidOperationException("Cannot delete past shows");
                }

                await _showRepository.RemoveAsync(show);
                await _showRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted show with ID: {ShowId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting show with ID: {ShowId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Show>> GetByMusicalIdAsync(int musicalId)
        {
            _logger.LogInformation("Getting shows by musical ID: {MusicalId}", musicalId);

            try
            {
                if (musicalId <= 0)
                {
                    _logger.LogWarning("Invalid musical ID provided: {MusicalId}", musicalId);
                    throw new ArgumentException("Invalid musical ID");
                }

                var shows = await _showRepository.GetByMusicalIdAsync(musicalId);
                _logger.LogInformation("Found {ShowCount} shows for musical {MusicalId}",
                    shows.Count(), musicalId);
                return shows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting shows by musical ID: {MusicalId}", musicalId);
                throw;
            }
        }

        public async Task<IEnumerable<Show>> GetUpcomingShowsAsync()
        {
            _logger.LogInformation("Getting upcoming shows");

            try
            {
                var shows = await _showRepository.GetUpcomingShowsAsync();
                _logger.LogInformation("Found {ShowCount} upcoming shows", shows.Count());
                return shows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting upcoming shows");
                throw;
            }
        }
    }
}