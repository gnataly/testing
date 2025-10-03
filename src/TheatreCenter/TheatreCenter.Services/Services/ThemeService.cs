using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TheatreCenter.Services.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IThemeRepository _themeRepository;

        public ThemeService(IThemeRepository themeRepository)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
        }

        public async Task<Theme> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid theme ID");

            var theme = await _themeRepository.GetByIdAsync(id);
            if (theme == null)
                throw new KeyNotFoundException("Theme not found");

            return theme;
        }

        public async Task<IEnumerable<Theme>> GetAllAsync()
        {
            return await _themeRepository.GetAllAsync();
        }

        public async Task<Theme> CreateAsync(Theme theme)
        {
            if (theme == null)
                throw new ArgumentNullException(nameof(theme));

            if (string.IsNullOrWhiteSpace(theme.Name))
                throw new ArgumentException("Theme name cannot be empty");

            await _themeRepository.AddAsync(theme);
            await _themeRepository.SaveChangesAsync();
            return theme;
        }

        public async Task<Theme> UpdateAsync(Theme theme)
        {
            if (theme == null)
                throw new ArgumentNullException(nameof(theme));

            var existingTheme = await _themeRepository.GetByIdAsync(theme.Id);
            if (existingTheme == null)
                throw new KeyNotFoundException("Theme not found");

            if (string.IsNullOrWhiteSpace(theme.Name))
                throw new ArgumentException("Theme name cannot be empty");

            existingTheme.Name = theme.Name;

            await _themeRepository.UpdateAsync(existingTheme);
            await _themeRepository.SaveChangesAsync();
            return existingTheme;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var theme = await _themeRepository.GetByIdAsync(id);
            if (theme == null)
                return false;

            if (theme.MusicalThemes.Any())
                throw new InvalidOperationException("Cannot delete theme assigned to musicals");

            await _themeRepository.RemoveAsync(theme);
            await _themeRepository.SaveChangesAsync();
            return true;
        }
    }
}