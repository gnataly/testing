using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Domain.Interfaces.Repositories
{
    public interface IMusicalRepository
    {
        Task<Musical?> GetByIdAsync(int id);
        Task<IEnumerable<Musical>> GetAllAsync();
        Task AddAsync(Musical musical);
        Task UpdateAsync(Musical musical);
        Task RemoveAsync(Musical musical);
        Task SaveChangesAsync();
        Task<IEnumerable<Musical>> GetByTheatreIdAsync(int theatreId);
        Task<IEnumerable<Musical>> GetByAgeRestrictionAsync(AgeRestriction ageRestriction);
    }
}
