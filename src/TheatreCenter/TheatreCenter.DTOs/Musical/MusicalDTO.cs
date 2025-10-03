using System.Text.Json.Serialization;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.DTOs.Musical;

public class MusicalDTO
{
    public MusicalDTO(int id, string title, string description, TimeSpan duration,
                     AgeRestriction ageRestriction, int theatreId/*,
                     DateTime updatedAt*/)
    {
        Id = id;
        Title = title;
        Description = description;
        Duration = duration;
        AgeRestriction = ageRestriction;
        TheatreId = theatreId;
        //UpdatedAt = updatedAt;
    }

    [JsonPropertyName("id")]
    public int Id { get; }

    [JsonPropertyName("title")]
    public string Title { get; }

    [JsonPropertyName("description")]
    public string Description { get; }

    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; }

    [JsonPropertyName("ageRestriction")]
    public AgeRestriction AgeRestriction { get; }

    

    [JsonPropertyName("theatreId")]
    public int TheatreId { get; }

    //[JsonPropertyName("updatedAt")]
    //public DateTime UpdatedAt { get; }
}