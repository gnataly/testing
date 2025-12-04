using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.Auth;

public class TwoFactorVerifyRequestDto
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("challengeId")]
    public string ChallengeId { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }
}

public class UnlockRequestDto
{
    [JsonPropertyName("username")]
    public string Username { get; set; }
}

public class UnlockVerifyRequestDto
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }
}

public class ChangePasswordRequestDto
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("currentPasswordHash")]
    public string CurrentPasswordHash { get; set; }

    [JsonPropertyName("newPasswordHash")]
    public string NewPasswordHash { get; set; }
}
