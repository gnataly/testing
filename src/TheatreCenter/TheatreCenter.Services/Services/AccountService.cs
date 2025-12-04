using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using Microsoft.Extensions.Configuration;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Services.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IConfiguration _configuration;

        public AccountService(IAccountRepository accountRepository, IConfiguration configuration)
        {
            _accountRepository = accountRepository;
            _configuration = configuration;
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _accountRepository.GetByIdAsync(id);
        }

        public async Task<Account?> GetByUsernameAsync(string username)
        {
            return await _accountRepository.GetByUsernameAsync(username);
        }

        public async Task<IEnumerable<Account>> GetAllAsync(AccountFilter filter)
        {
            return await _accountRepository.GetAllAsync(filter);
        }

        public async Task<Account> AuthenticateAsync(string username, string passwordHash)
        {
            var account = await _accountRepository.AuthenticateAsync(username, passwordHash);
            if (account == null)
                throw new UnauthorizedAccessException("Invalid username or password");

            return account;
        }

        public async Task<Account> RegisterAsync(string username, string passwordHash, AccessLevel accessLevel = AccessLevel.User)
        {
            var existingAccount = await _accountRepository.GetByUsernameAsync(username);
            if (existingAccount != null)
            {
                throw new ArgumentException("Username already exists");
            }

            var account = new Account(
                id: 0,
                username: username,
                passwordHash: passwordHash,
                accessLevel: accessLevel);

            await _accountRepository.CreateAsync(account);
            await _accountRepository.SaveChangesAsync();
            return account;
        }

        public async Task UpdateAsync(Account account)
        {
            var existingAccount = await _accountRepository.GetByIdAsync(account.Id);
            if (existingAccount == null)
                throw new KeyNotFoundException($"Account with id {account.Id} not found");

            existingAccount.Username = account.Username;
            existingAccount.PasswordHash = account.PasswordHash;

            await _accountRepository.UpdateAsync(existingAccount);
            await _accountRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int accountId)
        {
            await _accountRepository.DeleteAsync(accountId);
            await _accountRepository.SaveChangesAsync();
        }

        public async Task<bool> AddFavoriteActorAsync(int accountId, int actorId)
        {
            var result = await _accountRepository.AddFavoriteActorAsync(accountId, actorId);
            if (result)
            {
                await _accountRepository.SaveChangesAsync();
            }
            return result;
        }

        public async Task<bool> RemoveFavoriteActorAsync(int accountId, int actorId)
        {
            var result = await _accountRepository.RemoveFavoriteActorAsync(accountId, actorId);
            if (result)
            {
                await _accountRepository.SaveChangesAsync();
            }
            return result;
        }

        public async Task<bool> AddFavoriteMusicalAsync(int accountId, int musicalId)
        {
            var result = await _accountRepository.AddFavoriteMusicalAsync(accountId, musicalId);
            if (result)
            {
                await _accountRepository.SaveChangesAsync();
            }
            return result;
        }

        public async Task<bool> RemoveFavoriteMusicalAsync(int accountId, int musicalId)
        {
            var result = await _accountRepository.RemoveFavoriteMusicalAsync(accountId, musicalId);
            if (result)
            {
                await _accountRepository.SaveChangesAsync();
            }
            return result;
        }

        public async Task<bool> AddFavoriteTheatreAsync(int accountId, int theatreId)
        {
            var result = await _accountRepository.AddFavoriteTheatreAsync(accountId, theatreId);
            if (result)
            {
                await _accountRepository.SaveChangesAsync();
            }
            return result;
        }

        public async Task<bool> RemoveFavoriteTheatreAsync(int accountId, int theatreId)
        {
            var result = await _accountRepository.RemoveFavoriteTheatreAsync(accountId, theatreId);
            if (result)
            {
                await _accountRepository.SaveChangesAsync();
            }
            return result;
        }

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
            await _accountRepository.SaveChangesAsync();
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
