using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using Microsoft.Extensions.Logging;
using TheatreCenter.Data.Repositories;

namespace TheatreCenter.Services.Services
{
    public class MusicalService : IMusicalService
    {
        private readonly IMusicalRepository _musicalRepository;
        private readonly ITheatreRepository _theatreRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<MusicalService> _logger;

        public MusicalService(
            IMusicalRepository musicalRepository,
            ITheatreRepository theatreRepository,
            IAccountRepository accountRepository,
            ILogger<MusicalService> logger)
        {
            _musicalRepository = musicalRepository;
            _theatreRepository = theatreRepository;
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<Musical> GetMusicalByIdAsync(int id, int? currentUserId = null)
        {
            _logger.LogInformation("Attempting to get musical with ID: {MusicalId}", id);

            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid musical ID provided: {MusicalId}", id);
                    throw new ArgumentException("Invalid musical ID");
                }

                var musical = await _musicalRepository.GetByIdAsync(id);
                if (musical == null)
                {
                    _logger.LogWarning("Musical with ID {MusicalId} not found", id);
                    throw new KeyNotFoundException("Musical not found");
                }

                if (currentUserId.HasValue)
                {
                    musical.IsFavorite = await _accountRepository.IsFavoriteMusicalAsync(currentUserId.Value, id);
                }

                _logger.LogInformation("Successfully retrieved musical with ID: {MusicalId}", id);
                return musical;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting musical with ID: {MusicalId}", id);
                throw;
            }
        }

        public async Task<List<Musical>> GetAllMusicalsAsync(MusicalFilter filter, int? currentUserId = null)
        {
            _logger.LogInformation("Starting to retrieve all musicals with filter");

            try
            {
                var result = await _musicalRepository.GetAllAsync(filter);

                if (currentUserId.HasValue)
                {
                    foreach (var musical in result)
                    {
                        musical.IsFavorite = await _accountRepository.IsFavoriteMusicalAsync(currentUserId.Value, musical.Id);
                    }

                    if (filter.OnlyFavorites)
                    {
                        result = result.Where(m => m.IsFavorite).ToList();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all musicals");
                throw;
            }
        }


        public async Task<int> GetCountAsync(MusicalFilter filter, int? currentUserId = null)
        {
            _logger.LogInformation("Starting to retrieve all actors with filter");

            try
            {
                var result = await _musicalRepository.GetCountAsync(filter);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all actors");
                throw;
            }
        }

        public async Task<Musical> CreateMusicalAsync(Musical musical)
        {
            _logger.LogInformation("Attempting to create new musical");

            try
            {
                if (musical == null)
                {
                    _logger.LogError("Attempted to create null musical");
                    throw new ArgumentNullException(nameof(musical));
                }

                if (string.IsNullOrWhiteSpace(musical.Title))
                    throw new ArgumentException("Title cannot be empty");

                if (musical.Duration <= TimeSpan.Zero)
                    throw new ArgumentException("Duration must be positive");

                var theatre = await _theatreRepository.GetByIdAsync(musical.TheatreId);
                if (theatre == null)
                    throw new ArgumentException("Theatre not found");

                await _musicalRepository.AddAsync(musical);
                await _musicalRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully created new musical with ID: {MusicalId}", musical.Id);
                return musical;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating musical");
                throw;
            }
        }

        public async Task<Musical> UpdateMusicalAsync(Musical musical)
        {
            _logger.LogInformation("Attempting to update musical with ID: {MusicalId}", musical?.Id);

            try
            {
                if (musical == null)
                {
                    _logger.LogError("Attempted to update null musical");
                    throw new ArgumentNullException(nameof(musical));
                }

                var existingMusical = await _musicalRepository.GetByIdAsync(musical.Id);
                if (existingMusical == null)
                {
                    _logger.LogWarning("Musical with ID {MusicalId} not found for update", musical.Id);
                    throw new KeyNotFoundException("Musical not found");
                }

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

                _logger.LogInformation("Successfully updated musical with ID: {MusicalId}", musical.Id);
                return existingMusical;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating musical with ID: {MusicalId}", musical?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteMusicalAsync(int id)
        {
            _logger.LogInformation("Attempting to delete musical with ID: {MusicalId}", id);

            try
            {
                var musical = await _musicalRepository.GetByIdAsync(id);
                if (musical == null)
                {
                    _logger.LogWarning("Musical with ID {MusicalId} not found for deletion", id);
                    return false;
                }

                if (musical.Shows.Any())
                {
                    _logger.LogError("Cannot delete musical with ID {MusicalId} - has scheduled shows", id);
                    throw new InvalidOperationException("Cannot delete musical with scheduled shows");
                }

                await _musicalRepository.RemoveAsync(musical);
                await _musicalRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted musical with ID: {MusicalId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting musical with ID: {MusicalId}", id);
                throw;
            }
        }

        

        public async Task<bool> AddThemeToMusicalAsync(int musicalId, int themeId)
        {
            _logger.LogInformation("Adding theme {ThemeId} to musical {MusicalId}", themeId, musicalId);

            try
            {
                var result = await _musicalRepository.AddThemeToMusicalAsync(musicalId, themeId);
                if (result)
                {
                    await _musicalRepository.SaveChangesAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding theme to musical");
                throw;
            }
        }

        public async Task<bool> RemoveThemeFromMusicalAsync(int musicalId, int themeId)
        {
            _logger.LogInformation("Removing theme {ThemeId} from musical {MusicalId}", themeId, musicalId);

            try
            {
                var result = await _musicalRepository.RemoveThemeFromMusicalAsync(musicalId, themeId);
                if (result)
                {
                    await _musicalRepository.SaveChangesAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing theme from musical");
                throw;
            }
        }

        //public async Task<IEnumerable<Theme>> GetMusicalThemesAsync(int musicalId)
        //{
        //    _logger.LogInformation("Getting themes for musical {MusicalId}", musicalId);

        //    try
        //    {
        //        var themes = await _musicalRepository.GetMusicalThemesAsync(musicalId);
        //        _logger.LogInformation("Found {ThemeCount} themes for musical {MusicalId}",
        //            themes.Count(), musicalId);
        //        return themes;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting themes for musical {MusicalId}", musicalId);
        //        throw;
        //    }
        //}
    }
}