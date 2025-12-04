namespace TheatreCenter.Services.Models;

public class TwoFactorChallenge
{
    public required string ChallengeId { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required string DeliveryChannel { get; init; }
    public required DateTime? LockedUntil { get; init; }
}

public class UnlockChallenge
{
    public required DateTime ExpiresAt { get; init; }
}
