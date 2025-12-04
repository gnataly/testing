namespace TheatreCenter.Services.Options;

public class SecurityOptions
{
    public int TwoFactorCodeLength { get; set; } = 6;
    public int TwoFactorExpiryMinutes { get; set; } = 10;
    public int UnlockCodeExpiryMinutes { get; set; } = 15;
    public int MaxLoginAttempts { get; set; } = 5;
    public int MaxTwoFactorAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
}
