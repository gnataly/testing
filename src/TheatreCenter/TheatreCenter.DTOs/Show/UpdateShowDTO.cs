using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.Show;

public class UpdateShowDTO
{
    public UpdateShowDTO(DateTime date)
    {
        Date = date;
    }

    [JsonPropertyName("date")]
    public DateTime Date { get; }
}