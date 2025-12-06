using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs;

public class ThemeDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class ThemeListDto
{
    [JsonPropertyName("items")]
    public List<ThemeDto> Items { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationDto Pagination { get; set; }
}

public class CreateThemeRequestDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class UpdateThemeRequestDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}