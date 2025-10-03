using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.Theme;


public class CreateThemeDTO
{
    public CreateThemeDTO(string name)
    {
        Name = name;
    }

    [JsonPropertyName("name")]
    public string Name { get; }
}