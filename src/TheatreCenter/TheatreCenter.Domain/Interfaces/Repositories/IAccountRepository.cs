using TheatreCenter.Domain.Models;

namespace TheatreCenter.Domain.Interfaces.Repositories
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(int id);
        Task<Account?> GetByUsernameAsync(string username);
        Task<IEnumerable<Account>> GetAllAsync();
        Task CreateAsync(Account account);
        Task UpdateAsync(Account account);
        Task DeleteAsync(int id);
        Task<Account?> AuthenticateAsync(string username, string passwordHash);
        Task SaveChangesAsync();

        Task<bool> AddFavoriteActorAsync(int accountId, int actorId);
        Task<bool> RemoveFavoriteActorAsync(int accountId, int actorId);
        Task<bool> AddFavoriteMusicalAsync(int accountId, int musicalId);
        Task<bool> RemoveFavoriteMusicalAsync(int accountId, int musicalId);
        Task<bool> AddFavoriteTheatreAsync(int accountId, int theatreId);
        Task<bool> RemoveFavoriteTheatreAsync(int accountId, int theatreId);
        Task<AccountFavorites> GetFavoritesAsync(int accountId);
        Task<IEnumerable<Account>> GetAccountsWithUpgradeRequestAsync();
        Task<bool> ProcessUpgradeRequestAsync(int accountId, bool isApproved);

    }
}
