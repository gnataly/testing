using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.Theme;

public class UpdateThemeDTO
{
    public UpdateThemeDTO(string name)
    {
        Name = name;
    }

    [JsonPropertyName("name")]
    public string Name { get; }
}