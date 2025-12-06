using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using Microsoft.Extensions.Logging;

namespace TheatreCenter.Services.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IThemeRepository _themeRepository;
        private readonly ILogger<ThemeService> _logger;

        public ThemeService(
            IThemeRepository themeRepository,
            ILogger<ThemeService> logger)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _logger = logger;
        }

        public async Task<Theme> GetByIdAsync(int id)
        {
            _logger.LogInformation("Attempting to get theme with ID: {ThemeId}", id);

            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid theme ID provided: {ThemeId}", id);
                    throw new ArgumentException("Invalid theme ID");
                }

                var theme = await _themeRepository.GetByIdAsync(id);
                if (theme == null)
                {
                    _logger.LogWarning("Theme with ID {ThemeId} not found", id);
                    throw new KeyNotFoundException("Theme not found");
                }

                _logger.LogInformation("Successfully retrieved theme with ID: {ThemeId}", id);
                return theme;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting theme with ID: {ThemeId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Theme>> GetAllAsync(ThemeFilter filter)
        {
            _logger.LogInformation("Starting to retrieve all themes with filter");

            try
            {
                var result = await _themeRepository.GetAllAsync(filter);
                _logger.LogInformation("Retrieved {ThemeCount} themes", result.Count());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all themes");
                throw;
            }
        }

        public async Task<Theme> CreateAsync(Theme theme)
        {
            _logger.LogInformation("Attempting to create new theme");

            try
            {
                if (theme == null)
                {
                    _logger.LogError("Attempted to create null theme");
                    throw new ArgumentNullException(nameof(theme));
                }

                if (string.IsNullOrWhiteSpace(theme.Name))
                {
                    _logger.LogError("Attempted to create theme with empty name");
                    throw new ArgumentException("Theme name cannot be empty");
                }

                await _themeRepository.AddAsync(theme);
                await _themeRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully created new theme with ID: {ThemeId}", theme.Id);
                return theme;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating theme");
                throw;
            }
        }

        public async Task<Theme> UpdateAsync(Theme theme)
        {
            _logger.LogInformation("Attempting to update theme with ID: {ThemeId}", theme?.Id);

            try
            {
                if (theme == null)
                {
                    _logger.LogError("Attempted to update null theme");
                    throw new ArgumentNullException(nameof(theme));
                }

                var existingTheme = await _themeRepository.GetByIdAsync(theme.Id);
                if (existingTheme == null)
                {
                    _logger.LogWarning("Theme with ID {ThemeId} not found for update", theme.Id);
                    throw new KeyNotFoundException("Theme not found");
                }

                if (string.IsNullOrWhiteSpace(theme.Name))
                {
                    _logger.LogError("Attempted to update theme with empty name");
                    throw new ArgumentException("Theme name cannot be empty");
                }

                existingTheme.Name = theme.Name;

                await _themeRepository.UpdateAsync(existingTheme);
                await _themeRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully updated theme with ID: {ThemeId}", theme.Id);
                return existingTheme;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating theme with ID: {ThemeId}", theme?.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Attempting to delete theme with ID: {ThemeId}", id);

            try
            {
                var theme = await _themeRepository.GetByIdAsync(id);
                if (theme == null)
                {
                    _logger.LogWarning("Theme with ID {ThemeId} not found for deletion", id);
                    return false;
                }

                if (theme.MusicalThemes.Any())
                {
                    _logger.LogError("Cannot delete theme with ID {ThemeId} - assigned to musicals", id);
                    throw new InvalidOperationException("Cannot delete theme assigned to musicals");
                }

                await _themeRepository.RemoveAsync(theme);
                await _themeRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted theme with ID: {ThemeId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting theme with ID: {ThemeId}", id);
                throw;
            }
        }
    }
}