using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.Show;

public class CreateShowDTO
{
    public CreateShowDTO(DateTime date, int musicalId)
    {
        Date = date;
        MusicalId = musicalId;
    }

    [JsonPropertyName("date")]
    public DateTime Date { get; }

    [JsonPropertyName("musicalId")]
    public int MusicalId { get; }
}