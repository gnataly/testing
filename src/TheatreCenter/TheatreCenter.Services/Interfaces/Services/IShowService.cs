using TheatreCenter.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface IShowService
    {
        Task<Show> GetByIdAsync(int id);
        Task<IEnumerable<Show>> GetAllAsync();
        Task<Show> CreateAsync(Show show);
        Task<Show> UpdateAsync(Show show);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Show>> GetByMusicalIdAsync(int musicalId);
        Task<IEnumerable<Show>> GetUpcomingShowsAsync();
    }
}