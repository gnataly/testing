using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs;

public class ErrorDto
{
    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("details")]
    public string Details { get; set; }

    public ErrorDto(string error, string message, string details = null)
    {
        Error = error;
        Message = message;
        Details = details;
    }
}