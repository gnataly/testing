using Microsoft.EntityFrameworkCore;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Data.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _context.Accounts
                .Include(a => a.FavoriteActors)
                    .ThenInclude(f => f.Actor)
                .Include(a => a.FavoriteMusicals)
                    .ThenInclude(f => f.Musical)
                .Include(a => a.FavoriteTheatres)
                    .ThenInclude(f => f.Theatre)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Account?> GetByUsernameAsync(string username)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == username);
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _context.Accounts.ToListAsync();
        }

        public async Task CreateAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
        }

        public async Task UpdateAsync(Account account)
        {
            _context.Accounts.Update(account);
        }

        public async Task DeleteAsync(int id)
        {
            var account = await GetByIdAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }
        }

        public async Task<Account?> AuthenticateAsync(string username, string passwordHash)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == username && a.PasswordHash == passwordHash);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AddFavoriteActorAsync(int accountId, int actorId)
        {
            var account = await _context.Accounts
                .Include(a => a.FavoriteActors)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            var actor = await _context.Actors.FindAsync(actorId);

            if (account == null || actor == null) return false;

            if (account.FavoriteActors.Any(f => f.ActorId == actorId))
                return false;

            account.FavoriteActors.Add(new AccountActorFavorite(accountId, actorId));
            return true;
        }

        public async Task<bool> RemoveFavoriteActorAsync(int accountId, int actorId)
        {
            var account = await _context.Accounts
                .Include(a => a.FavoriteActors)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null) return false;

            var favorite = account.FavoriteActors.FirstOrDefault(f => f.ActorId == actorId);
            if (favorite == null) return false;

            account.FavoriteActors.Remove(favorite);
            return true;
        }

        public async Task<bool> AddFavoriteMusicalAsync(int accountId, int musicalId)
        {
            var account = await _context.Accounts
                .Include(a => a.FavoriteMusicals)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            var musical = await _context.Musicals.FindAsync(musicalId);

            if (account == null || musical == null) return false;

            if (account.FavoriteMusicals.Any(f => f.MusicalId == musicalId))
                return false;

            account.FavoriteMusicals.Add(new AccountMusicalFavorite(accountId, musicalId));
            return true;
        }

        public async Task<bool> RemoveFavoriteMusicalAsync(int accountId, int musicalId)
        {
            var account = await _context.Accounts
                .Include(a => a.FavoriteMusicals)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null) return false;

            var favorite = account.FavoriteMusicals.FirstOrDefault(f => f.MusicalId == musicalId);
            if (favorite == null) return false;

            account.FavoriteMusicals.Remove(favorite);
            return true;
        }

        public async Task<bool> AddFavoriteTheatreAsync(int accountId, int theatreId)
        {
            var account = await _context.Accounts
                .Include(a => a.FavoriteTheatres)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            var theatre = await _context.Theatres.FindAsync(theatreId);

            if (account == null || theatre == null) return false;

            if (account.FavoriteTheatres.Any(f => f.TheatreId == theatreId))
                return false;

            account.FavoriteTheatres.Add(new AccountTheatreFavorite(accountId, theatreId));
            return true;
        }

        public async Task<bool> RemoveFavoriteTheatreAsync(int accountId, int theatreId)
        {
            var account = await _context.Accounts
                .Include(a => a.FavoriteTheatres)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null) return false;

            var favorite = account.FavoriteTheatres.FirstOrDefault(f => f.TheatreId == theatreId);
            if (favorite == null) return false;

            account.FavoriteTheatres.Remove(favorite);
            return true;
        }

        public async Task<AccountFavorites> GetFavoritesAsync(int accountId)
        {
            var account = await _context.Accounts
                .Include(a => a.FavoriteActors)
                    .ThenInclude(f => f.Actor)
                .Include(a => a.FavoriteMusicals)
                    .ThenInclude(f => f.Musical)
                .Include(a => a.FavoriteTheatres)
                    .ThenInclude(f => f.Theatre)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null) return null;

            return new AccountFavorites
            {
                Actors = account.FavoriteActors.Select(f => f.Actor).ToList(),
                Musicals = account.FavoriteMusicals.Select(f => f.Musical).ToList(),
                Theatres = account.FavoriteTheatres.Select(f => f.Theatre).ToList()
            };
        }

        public async Task<IEnumerable<Account>> GetAccountsWithUpgradeRequestAsync()
        {
            return await _context.Accounts
                .Where(a => a.UpgradeRequest)
                .ToListAsync();
        }

        public async Task<bool> ProcessUpgradeRequestAsync(int accountId, bool isApproved)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) return false;

            account.UpgradeRequest = false;
            if (isApproved)
            {
                account.AccessLevel = AccessLevel.Admin;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}