using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.Theme;

public class ThemeDTO
{
    public ThemeDTO(int id, string name)
    {
        Id = id;
        Name = name;
    }

    [JsonPropertyName("id")]
    public int Id { get; }

    [JsonPropertyName("name")]
    public string Name { get; }
}