using System.Text.Json.Serialization;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.DTOs;

public class AuthRequestDto
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("passwordHash")]
    public string PasswordHash { get; set; }
}

public class AuthResponseDto
{
    [JsonPropertyName("token")]
    public string Token { get; set; }

    [JsonPropertyName("account")]
    public AccountDto Account { get; set; }

    [JsonPropertyName("requiresTwoFactor")]
    public bool RequiresTwoFactor { get; set; }

    [JsonPropertyName("twoFactorChallengeId")]
    public string TwoFactorChallengeId { get; set; }

    [JsonPropertyName("twoFactorExpiresAt")]
    public DateTime? TwoFactorExpiresAt { get; set; }

    [JsonPropertyName("deliveryChannel")]
    public string DeliveryChannel { get; set; }

    [JsonPropertyName("lockedUntil")]
    public DateTime? LockedUntil { get; set; }
}
