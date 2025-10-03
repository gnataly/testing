using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Domain.Enums;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Services.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(
            IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _accountRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _accountRepository.GetAllAsync();
        }

        public async Task<Account> AuthenticateAsync(string username, string passwordHash)
        {
            var account = await _accountRepository.AuthenticateAsync(username, passwordHash);
            if (account == null || account.PasswordHash != passwordHash)
                throw new UnauthorizedAccessException("Invalid username or password");

            return account;
        }


        public async Task<Account> RegisterAsync(string username, string passwordHash, AccessLevel accessLevel = AccessLevel.User)
        {
            // Проверка существования пользователя

            var existingAccount = await _accountRepository.GetByUsernameAsync(username);
            if (existingAccount != null)
            {
                throw new ArgumentException("Username already exists");
            }

            var account = new Account(
                id: 0,
                username: username,
                passwordHash: passwordHash,
                lastFavoritesViewDate: DateTime.UtcNow,
                accessLevel: accessLevel);


            await _accountRepository.CreateAsync(account);
            return account;
        }

        public async Task LogoutAsync(int accountId)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account != null)
            {
                account.UpdateLastViewDate();
                await _accountRepository.UpdateAsync(account);
            }
        }

        public async Task UpdateAsync(Account account)
        {
            var existingAccount = await _accountRepository.GetByIdAsync(account.Id);
            if (existingAccount == null)
                throw new KeyNotFoundException($"Account with id {account.Id} not found");

            await _accountRepository.UpdateAsync(account);
        }

        public async Task DeleteAsync(int accountId)
        {
            await _accountRepository.DeleteAsync(accountId);
        }


        //public async Task<bool> AddFavoriteActorAsync(int accountId, int actorId)
        //{
        //    var account = await _accountRepository.GetByIdAsync(accountId);
        //    var actor = await _actorRepository.GetByIdAsync(actorId);

        //    if (account == null || actor == null)
        //        return false;

        //    if (account.FavoriteActors.Any(f => f.ActorId == actorId))
        //        return false;

        //    account.FavoriteActors.Add(new AccountActorFavorite(accountId, actorId));
        //    account.UpdateLastViewDate();
        //    await _accountRepository.SaveChangesAsync();
        //    return true;
        //}

        //public async Task<bool> RemoveFavoriteActorAsync(int accountId, int actorId)
        //{
        //    var account = await _accountRepository.GetByIdAsync(accountId);
        //    if (account == null) return false;

        //    var favorite = account.FavoriteActors.FirstOrDefault(f => f.ActorId == actorId);
        //    if (favorite == null) return false;

        //    account.FavoriteActors.Remove(favorite);
        //    await _accountRepository.SaveChangesAsync();
        //    return true;
        //}

        public async Task ChangePasswordAsync(int accountId, string newPasswordHash)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null)
                throw new ArgumentException("Account not found");

            account.ChangePassword(newPasswordHash);
            await _accountRepository.SaveChangesAsync();
        }

        public async Task UpdateLastFavoritesViewDateAsync(int accountId)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account != null)
            {
                account.UpdateLastViewDate();
                await _accountRepository.SaveChangesAsync();
            }
        }

        //public async Task<bool> AddFavoriteMusicalAsync(int accountId, int musicalId)
        //{
        //    var account = await _accountRepository.GetByIdAsync(accountId);
        //    var musical = await _musicalRepository.GetByIdAsync(musicalId);

        //    if (account == null || musical == null)
        //        return false;

        //    if (account.FavoriteMusicals.Any(f => f.MusicalId == musicalId))
        //        return false;

        //    account.FavoriteMusicals.Add(new AccountMusicalFavorite(accountId, musicalId));
        //    account.UpdateLastViewDate();
        //    await _accountRepository.SaveChangesAsync();
        //    return true;
        //}

        //public async Task<bool> RemoveFavoriteMusicalAsync(int accountId, int musicalId)
        //{
        //    var account = await _accountRepository.GetByIdAsync(accountId);
        //    if (account == null) return false;

        //    var favorite = account.FavoriteMusicals.FirstOrDefault(f => f.MusicalId == musicalId);
        //    if (favorite == null) return false;

        //    account.FavoriteMusicals.Remove(favorite);
        //    await _accountRepository.SaveChangesAsync();
        //    return true;
        //}

        //public async Task<bool> AddFavoriteTheatreAsync(int accountId, int theatreId)
        //{
        //    var account = await _accountRepository.GetByIdAsync(accountId);
        //    var theatre = await _theatreRepository.GetByIdAsync(theatreId);

        //    if (account == null || theatre == null)
        //        return false;

        //    if (account.FavoriteTheatres.Any(f => f.TheatreId == theatreId))
        //        return false;

        //    account.FavoriteTheatres.Add(new AccountTheatreFavorite(accountId, theatreId));
        //    account.UpdateLastViewDate();
        //    await _accountRepository.SaveChangesAsync();
        //    return true;
        //}

        //public async Task<bool> RemoveFavoriteTheatreAsync(int accountId, int theatreId)
        //{
        //    var account = await _accountRepository.GetByIdAsync(accountId);
        //    if (account == null) return false;

        //    var favorite = account.FavoriteTheatres.FirstOrDefault(f => f.TheatreId == theatreId);
        //    if (favorite == null) return false;

        //    account.FavoriteTheatres.Remove(favorite);
        //    await _accountRepository.SaveChangesAsync();
        //    return true;
        //}


        public async Task<bool> AddFavoriteActorAsync(int accountId, int actorId)
        {
            return await _accountRepository.AddFavoriteActorAsync(accountId, actorId);
        }

        public async Task<bool> RemoveFavoriteActorAsync(int accountId, int actorId)
        {
            return await _accountRepository.RemoveFavoriteActorAsync(accountId, actorId);
        }

        public async Task<bool> AddFavoriteMusicalAsync(int accountId, int musicalId)
        {
            return await _accountRepository.AddFavoriteMusicalAsync(accountId, musicalId);
        }

        public async Task<bool> RemoveFavoriteMusicalAsync(int accountId, int musicalId)
        {
            return await _accountRepository.RemoveFavoriteMusicalAsync(accountId, musicalId);
        }

        public async Task<bool> AddFavoriteTheatreAsync(int accountId, int theatreId)
        {
            return await _accountRepository.AddFavoriteTheatreAsync(accountId, theatreId);
        }

        public async Task<bool> RemoveFavoriteTheatreAsync(int accountId, int theatreId)
        {
            return await _accountRepository.RemoveFavoriteTheatreAsync(accountId, theatreId);
        }

        public async Task<AccountFavorites> GetFavoritesAsync(int accountId)
        {
            return await _accountRepository.GetFavoritesAsync(accountId);
        }

        public async Task<bool> SubmitUpgradeRequestAsync(int accountId)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) return false;

            account.UpgradeRequest = true;
            await _accountRepository.UpdateAsync(account);
            return true;
        }

        public async Task<IEnumerable<Account>> GetAccountsWithUpgradeRequestAsync()
        {
            return await _accountRepository.GetAccountsWithUpgradeRequestAsync();
        }

        public async Task<bool> ProcessUpgradeRequestAsync(int accountId, bool isApproved)
        {
            return await _accountRepository.ProcessUpgradeRequestAsync(accountId, isApproved);
        }
    }
}
