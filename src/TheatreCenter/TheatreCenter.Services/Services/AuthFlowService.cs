using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheatreCenter.Domain.Interfaces.Repositories;
using TheatreCenter.Domain.Models;
using TheatreCenter.Services.Interfaces.Services;
using TheatreCenter.Services.Models;
using TheatreCenter.Services.Options;

namespace TheatreCenter.Services.Services;

public class AuthFlowService : IAuthFlowService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IEmailSender _emailSender;
    private readonly SecurityOptions _securityOptions;
    private readonly ILogger<AuthFlowService> _logger;

    public AuthFlowService(
        IAccountRepository accountRepository,
        IEmailSender emailSender,
        IOptions<SecurityOptions> securityOptions,
        ILogger<AuthFlowService> logger)
    {
        _accountRepository = accountRepository;
        _emailSender = emailSender;
        _logger = logger;
        _securityOptions = securityOptions.Value;
    }

    public async Task<TwoFactorChallenge> StartTwoFactorLoginAsync(string username, string passwordHash)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        if (IsLocked(account))
        {
            throw new LockoutException(account.LockedUntil!.Value);
        }

        if (account.PasswordHash != passwordHash)
        {
            await RegisterLoginFailureAsync(account);
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        ResetLoginFailures(account);
        var code = GenerateNumericCode(_securityOptions.TwoFactorCodeLength);
        var challengeId = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddMinutes(_securityOptions.TwoFactorExpiryMinutes);

        account.PendingTwoFactorChallengeId = challengeId;
        account.PendingTwoFactorCodeHash = Hash(code);
        account.PendingTwoFactorExpiresAt = expiresAt;
        account.FailedTwoFactorAttempts = 0;

        await _accountRepository.SaveChangesAsync();
        await _emailSender.SendTwoFactorCodeAsync(account.Username, code, expiresAt);

        return new TwoFactorChallenge
        {
            ChallengeId = challengeId,
            ExpiresAt = expiresAt,
            DeliveryChannel = "email",
            LockedUntil = account.LockedUntil
        };
    }

    public async Task<Account> VerifyTwoFactorAsync(string username, string challengeId, string code)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
        {
            throw new UnauthorizedAccessException("Invalid challenge");
        }

        if (IsLocked(account))
        {
            throw new LockoutException(account.LockedUntil!.Value);
        }

        if (account.PendingTwoFactorChallengeId != challengeId || account.PendingTwoFactorExpiresAt < DateTime.UtcNow)
        {
            await RegisterTwoFactorFailureAsync(account);
            throw new UnauthorizedAccessException("Invalid or expired code");
        }

        if (!SlowEquals(account.PendingTwoFactorCodeHash, Hash(code)))
        {
            await RegisterTwoFactorFailureAsync(account);
            throw new UnauthorizedAccessException("Invalid two factor code");
        }

        ClearPendingChallenges(account);
        await _accountRepository.SaveChangesAsync();
        return account;
    }

    public async Task<UnlockChallenge> SendUnlockCodeAsync(string username)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
        {
            throw new UnauthorizedAccessException("Account not found");
        }

        if (!IsLocked(account))
        {
            return new UnlockChallenge
            {
                ExpiresAt = DateTime.UtcNow.AddMinutes(_securityOptions.UnlockCodeExpiryMinutes)
            };
        }

        var code = GenerateNumericCode(_securityOptions.TwoFactorCodeLength);
        var expiresAt = DateTime.UtcNow.AddMinutes(_securityOptions.UnlockCodeExpiryMinutes);

        account.PendingUnlockCodeHash = Hash(code);
        account.PendingUnlockCodeExpiresAt = expiresAt;
        await _accountRepository.SaveChangesAsync();

        await _emailSender.SendUnlockCodeAsync(account.Username, code, expiresAt);
        return new UnlockChallenge { ExpiresAt = expiresAt };
    }

    public async Task<Account> VerifyUnlockCodeAsync(string username, string code)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
        {
            throw new UnauthorizedAccessException("Account not found");
        }

        if (account.PendingUnlockCodeExpiresAt < DateTime.UtcNow ||
            !SlowEquals(account.PendingUnlockCodeHash, Hash(code)))
        {
            throw new UnauthorizedAccessException("Invalid or expired unlock code");
        }

        ResetLockout(account);
        ClearPendingChallenges(account);
        account.PendingUnlockCodeHash = null;
        account.PendingUnlockCodeExpiresAt = null;
        await _accountRepository.SaveChangesAsync();

        return account;
    }

    public async Task<Account> ChangePasswordAsync(string username, string currentPasswordHash, string newPasswordHash)
    {
        var account = await _accountRepository.GetByUsernameAsync(username);
        if (account == null)
        {
            throw new UnauthorizedAccessException("Account not found");
        }

        if (IsLocked(account))
        {
            throw new LockoutException(account.LockedUntil!.Value);
        }

        if (account.PasswordHash != currentPasswordHash)
        {
            await RegisterLoginFailureAsync(account);
            throw new UnauthorizedAccessException("Invalid current password");
        }

        account.ChangePassword(newPasswordHash);
        ResetLoginFailures(account);
        ClearPendingChallenges(account);
        await _accountRepository.SaveChangesAsync();
        return account;
    }

    private async Task RegisterLoginFailureAsync(Account account)
    {
        account.FailedLoginAttempts++;
        await ApplyLockIfNeededAsync(account);
        await _accountRepository.SaveChangesAsync();
    }

    private async Task RegisterTwoFactorFailureAsync(Account account)
    {
        account.FailedTwoFactorAttempts++;
        await ApplyLockIfNeededAsync(account);
        await _accountRepository.SaveChangesAsync();
    }

    private async Task ApplyLockIfNeededAsync(Account account)
    {
        if (account.FailedLoginAttempts >= _securityOptions.MaxLoginAttempts ||
            account.FailedTwoFactorAttempts >= _securityOptions.MaxTwoFactorAttempts)
        {
            account.LockedUntil = DateTime.UtcNow.AddMinutes(_securityOptions.LockoutMinutes);
            await _emailSender.SendLockoutNotificationAsync(account.Username, account.LockedUntil.Value);
            _logger.LogWarning("Account {Username} locked until {LockedUntil}", account.Username, account.LockedUntil);
        }
    }

    private static void ResetLockout(Account account)
    {
        account.LockedUntil = null;
        account.FailedLoginAttempts = 0;
        account.FailedTwoFactorAttempts = 0;
    }

    private static void ResetLoginFailures(Account account)
    {
        account.FailedLoginAttempts = 0;
    }

    private static void ClearPendingChallenges(Account account)
    {
        account.PendingTwoFactorChallengeId = null;
        account.PendingTwoFactorCodeHash = null;
        account.PendingTwoFactorExpiresAt = null;
        account.PendingUnlockCodeExpiresAt = null;
        account.PendingUnlockCodeHash = null;
        account.FailedTwoFactorAttempts = 0;
    }

    private static bool IsLocked(Account account)
    {
        return account.LockedUntil.HasValue && account.LockedUntil.Value > DateTime.UtcNow;
    }

    private static string GenerateNumericCode(int length)
    {
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);

        var builder = new StringBuilder(length);
        foreach (var b in bytes)
        {
            builder.Append((b % 10).ToString());
        }

        return builder.ToString();
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }

    private static bool SlowEquals(string? a, string? b)
    {
        if (a == null || b == null) return false;
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);

        if (aBytes.Length != bBytes.Length) return false;

        var diff = 0;
        for (var i = 0; i < aBytes.Length; i++)
        {
            diff |= aBytes[i] ^ bBytes[i];
        }

        return diff == 0;
    }
}

public class LockoutException : Exception
{
    public DateTime LockedUntil { get; }

    public LockoutException(DateTime lockedUntil) : base($"Account locked until {lockedUntil:O}")
    {
        LockedUntil = lockedUntil;
    }
}
