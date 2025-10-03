using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Services.Services
{
    public class TheatreService : ITheatreService
    {
        private readonly ITheatreRepository _theatreRepository;

        public TheatreService(ITheatreRepository theatreRepository)
        {
            _theatreRepository = theatreRepository ?? throw new ArgumentNullException(nameof(theatreRepository));
        }

        public async Task<Theatre?> GetTheatreByIdAsync(int id)
        {
            return await _theatreRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Theatre>> GetAllTheatresAsync()
        {
            return await _theatreRepository.GetAllAsync();
        }

        public async Task<Theatre> CreateTheatreAsync(Theatre theatre)
        {
            if (theatre == null)
                throw new ArgumentNullException(nameof(theatre));

            await _theatreRepository.AddAsync(theatre);
            await _theatreRepository.SaveChangesAsync();
            return theatre;
        }

        public async Task<Theatre> UpdateTheatreAsync(Theatre theatre)
        {
            if (theatre == null)
                throw new ArgumentNullException(nameof(theatre));

            var existingTheatre = await _theatreRepository.GetByIdAsync(theatre.Id);
            if (existingTheatre == null)
                throw new KeyNotFoundException($"Theatre with id {theatre.Id} not found");

            await _theatreRepository.UpdateAsync(theatre);
            await _theatreRepository.SaveChangesAsync();
            return theatre;
        }

        public async Task<bool> DeleteTheatreAsync(int id)
        {
            var theatre = await _theatreRepository.GetByIdAsync(id);
            if (theatre == null)
                return false;

            await _theatreRepository.RemoveAsync(theatre);
            await _theatreRepository.SaveChangesAsync();
            return true;
        }
    }
}
