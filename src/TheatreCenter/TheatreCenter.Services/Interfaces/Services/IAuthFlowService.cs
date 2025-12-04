using TheatreCenter.Domain.Models;
using TheatreCenter.Services.Models;

namespace TheatreCenter.Services.Interfaces.Services;

public interface IAuthFlowService
{
    Task<TwoFactorChallenge> StartTwoFactorLoginAsync(string username, string passwordHash);
    Task<Account> VerifyTwoFactorAsync(string username, string challengeId, string code);
    Task<UnlockChallenge> SendUnlockCodeAsync(string username);
    Task<Account> VerifyUnlockCodeAsync(string username, string code);
    Task<Account> ChangePasswordAsync(string username, string currentPasswordHash, string newPasswordHash);
}
