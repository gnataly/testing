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
}
