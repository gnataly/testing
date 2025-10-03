using System.Text.Json.Serialization;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.DTOs.Musical;

public class UpdateMusicalDTO
{
    public UpdateMusicalDTO(string description, TimeSpan duration,
                          AgeRestriction ageRestriction, int theatreId)
    {
        Description = description;
        Duration = duration;
        AgeRestriction = ageRestriction;
        TheatreId = theatreId;
    }

    [JsonPropertyName("description")]
    public string Description { get; }

    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; }

    [JsonPropertyName("ageRestriction")]
    public AgeRestriction AgeRestriction { get; }

    [JsonPropertyName("theatreId")]
    public int TheatreId { get; }
}