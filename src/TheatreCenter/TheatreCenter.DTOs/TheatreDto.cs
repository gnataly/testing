using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs;

public class TheatreDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("addInfo")]
    public string AddInfo { get; set; }

    [JsonPropertyName("isFavorite")]
    public bool IsFavorite { get; set; }
}

public class TheatreListDto
{
    [JsonPropertyName("items")]
    public List<TheatreDto> Items { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationDto Pagination { get; set; }
}

public class CreateTheatreRequestDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("addInfo")]
    public string AddInfo { get; set; }
}

public class UpdateTheatreRequestDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("addInfo")]
    public string AddInfo { get; set; }
}