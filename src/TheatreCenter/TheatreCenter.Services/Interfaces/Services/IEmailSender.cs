namespace TheatreCenter.Services.Interfaces.Services;

public interface IEmailSender
{
    Task SendTwoFactorCodeAsync(string recipientEmail, string code, DateTime expiresAt);
    Task SendUnlockCodeAsync(string recipientEmail, string code, DateTime expiresAt);
    Task SendLockoutNotificationAsync(string recipientEmail, DateTime lockedUntil);
}
