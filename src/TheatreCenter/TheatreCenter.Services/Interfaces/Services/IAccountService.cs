using TheatreCenter;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Services.Interfaces.Services
{
    public interface IAccountService
    {
        Task<Account?> GetByIdAsync(int id);
        Task<IEnumerable<Account>> GetAllAsync();
        Task<Account?> AuthenticateAsync(string username, string passwordHash);
        Task<Account> RegisterAsync(string username, string passwordHash, AccessLevel accessLevel = AccessLevel.User);
        Task LogoutAsync(int accountId);
        Task UpdateAsync(Account account);
        Task DeleteAsync(int accountId);
        Task<bool> AddFavoriteActorAsync(int accountId, int actorId);
        Task<bool> RemoveFavoriteActorAsync(int accountId, int actorId);
        Task<bool> AddFavoriteMusicalAsync(int accountId, int musicalId);
        Task<bool> RemoveFavoriteMusicalAsync(int accountId, int musicalId);
        Task<bool> AddFavoriteTheatreAsync(int accountId, int theatreId);
        Task<bool> RemoveFavoriteTheatreAsync(int accountId, int theatreId);
        Task UpdateLastFavoritesViewDateAsync(int accountId);
        Task ChangePasswordAsync(int accountId, string newPasswordHash);
        Task<AccountFavorites> GetFavoritesAsync(int accountId);
        Task<bool> SubmitUpgradeRequestAsync(int accountId);
        Task<IEnumerable<Account>> GetAccountsWithUpgradeRequestAsync();
        Task<bool> ProcessUpgradeRequestAsync(int accountId, bool isApproved);

    }
}
