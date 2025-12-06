using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs;

public class FavoriteResponseDto
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    public FavoriteResponseDto(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}
