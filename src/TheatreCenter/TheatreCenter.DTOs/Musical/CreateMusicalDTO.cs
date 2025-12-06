//using System.Text.Json.Serialization;
//using TheatreCenter.Domain.Enums;

//namespace TheatreCenter.DTOs;

//public class CreateMusicalDto
//{
//    public CreateMusicalDto(string title, string description, TimeSpan duration,
//                          AgeRestriction ageRestriction, int theatreId)
//    {
//        Title = title;
//        Description = description;
//        Duration = duration;
//        AgeRestriction = ageRestriction;
//        TheatreId = theatreId;
//    }

//    [JsonPropertyName("title")]
//    public string Title { get; }

//    [JsonPropertyName("description")]
//    public string Description { get; }

//    [JsonPropertyName("duration")]
//    public TimeSpan Duration { get; }

//    [JsonPropertyName("ageRestriction")]
//    public AgeRestriction AgeRestriction { get; }

//    [JsonPropertyName("theatreId")]
//    public int TheatreId { get; }
//}
