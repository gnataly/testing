using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using Microsoft.Extensions.Logging;
using TheatreCenter.Data.Repositories;

namespace TheatreCenter.Services.Services
{
    public class TheatreService : ITheatreService
    {
        private readonly ITheatreRepository _theatreRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<TheatreService> _logger;

        public TheatreService(
            ITheatreRepository theatreRepository,
            IAccountRepository accountRepository,
            ILogger<TheatreService> logger)
        {
            _theatreRepository = theatreRepository ?? throw new ArgumentNullException(nameof(theatreRepository));
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<Theatre?> GetTheatreByIdAsync(int id, int? currentUserId = null)
        {
            _logger.LogInformation("Attempting to get theatre with ID: {TheatreId}", id);

            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid theatre ID provided: {TheatreId}", id);
                    throw new ArgumentException("Invalid theatre ID");
                }

                var theatre = await _theatreRepository.GetByIdAsync(id);
                if (theatre == null)
                {
                    _logger.LogWarning("Theatre with ID {TheatreId} not found", id);
                    throw new KeyNotFoundException("Theatre not found");
                }

                if (currentUserId.HasValue)
                {
                    theatre.IsFavorite = await _accountRepository.IsFavoriteTheatreAsync(currentUserId.Value, id);
                }

                _logger.LogInformation("Successfully retrieved theatre with ID: {TheatreId}", id);
                return theatre;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting theatre with ID: {TheatreId}", id);
                throw;
            }
        }

        public async Task<List<Theatre>> GetAllTheatresAsync(TheatreFilter filter, int? currentUserId = null)
        {
            _logger.LogInformation("Starting to retrieve all theatres with filter");

            try
            {
                var result = await _theatreRepository.GetAllAsync(filter);

                if (currentUserId.HasValue)
                {
                    foreach (var theatre in result)
                    {
                        theatre.IsFavorite = await _accountRepository.IsFavoriteTheatreAsync(currentUserId.Value, theatre.Id);
                    }

                    if (filter.OnlyFavorites)
                    {
                        result = result.Where(t => t.IsFavorite).ToList();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all theatres");
                throw;
            }
        }


        public async Task<int> GetCountAsync(TheatreFilter filter, int? currentUserId = null)
        {
            _logger.LogInformation("Starting to retrieve all actors with filter");

            try
            {
                var result = await _theatreRepository.GetCountAsync(filter);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all actors");
                throw;
            }
        }

        public async Task<Theatre> CreateTheatreAsync(Theatre theatre)
        {
            _logger.LogInformation("Attempting to create new theatre");

            try
            {
                if (theatre == null)
                {
                    _logger.LogError("Attempted to create null theatre");
                    throw new ArgumentNullException(nameof(theatre));
                }

                await _theatreRepository.AddAsync(theatre);
                await _theatreRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully created new theatre with ID: {TheatreId}", theatre.Id);
                return theatre;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating theatre");
                throw;
            }
        }

        public async Task<Theatre> UpdateTheatreAsync(Theatre theatre)
        {
            _logger.LogInformation("Attempting to update theatre with ID: {TheatreId}", theatre?.Id);

            try
            {
                if (theatre == null)
                {
                    _logger.LogError("Attempted to update null theatre");
                    throw new ArgumentNullException(nameof(theatre));
                }

                var existingTheatre = await _theatreRepository.GetByIdAsync(theatre.Id);
                if (existingTheatre == null)
                {
                    _logger.LogWarning("Theatre with ID {TheatreId} not found for update", theatre.Id);
                    throw new KeyNotFoundException($"Theatre with id {theatre.Id} not found");
                }

                existingTheatre.Name = theatre.Name;
                existingTheatre.AddInfo = theatre.AddInfo;

                await _theatreRepository.UpdateAsync(existingTheatre);

                await _theatreRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully updated theatre with ID: {TheatreId}", theatre.Id);
                return theatre;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating theatre with ID: {TheatreId}", theatre?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteTheatreAsync(int id)
        {
            _logger.LogInformation("Attempting to delete theatre with ID: {TheatreId}", id);

            try
            {
                var theatre = await _theatreRepository.GetByIdAsync(id);
                if (theatre == null)
                {
                    _logger.LogWarning("Theatre with ID {TheatreId} not found for deletion", id);
                    return false;
                }

                await _theatreRepository.RemoveAsync(theatre);
                await _theatreRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted theatre with ID: {TheatreId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting theatre with ID: {TheatreId}", id);
                throw;
            }
        }
    }
}