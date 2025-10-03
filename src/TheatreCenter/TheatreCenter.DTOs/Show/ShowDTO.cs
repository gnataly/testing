using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.Show;

public class ShowDTO
{
    public ShowDTO(int id, DateTime date, int musicalId/*, DateTime updatedAt*/)
    {
        Id = id;
        Date = date;
        MusicalId = musicalId;
        //UpdatedAt = updatedAt;
    }

    [JsonPropertyName("id")]
    public int Id { get; }

    [JsonPropertyName("date")]
    public DateTime Date { get; }

    [JsonPropertyName("musicalId")]
    public int MusicalId { get; }

    //[JsonPropertyName("updatedAt")]
    //public DateTime UpdatedAt { get; }
}

