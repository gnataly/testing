using System.Text.Json.Serialization;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.DTOs;

public class MusicalDto
{

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; set; }

    [JsonPropertyName("ageRestriction")]
    public AgeRestriction AgeRestriction { get; set; }

    [JsonPropertyName("theatreId")]
    public int TheatreId { get; set; }

    [JsonPropertyName("isFavorite")]
    public bool IsFavorite { get; set; }
}

public class MusicalListDto
{
    [JsonPropertyName("items")]
    public List<MusicalDto> Items { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationDto Pagination { get; set; }
}

public class CreateMusicalRequestDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; set; }

    [JsonPropertyName("ageRestriction")]
    public AgeRestriction AgeRestriction { get; set; }

    [JsonPropertyName("theatreId")]
    public int TheatreId { get; set; }
}

public class UpdateMusicalRequestDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; set; }

    [JsonPropertyName("ageRestriction")]
    public AgeRestriction AgeRestriction { get; set; }

    [JsonPropertyName("theatreId")]
    public int TheatreId { get; set; }
}
