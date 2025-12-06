using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs;

public class ShowDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("musicalId")]
    public int MusicalId { get; set; }
}

public class ShowListDto
{
    [JsonPropertyName("items")]
    public List<ShowDto> Items { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationDto Pagination { get; set; }
}

public class CreateShowRequestDto
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("musicalId")]
    public int MusicalId { get; set; }
}

public class UpdateShowRequestDto
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
}