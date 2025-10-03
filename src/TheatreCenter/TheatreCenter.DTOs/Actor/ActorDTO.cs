
namespace TheatreCenter.DTOs.Actor;

using System.Text.Json.Serialization;
using TheatreCenter.Domain.Enums;

public class ActorDTO
{
    public ActorDTO(int id, string name, VoiceType voiceType, Gender gender,
                  DateTime birthDate, int height, int weight, string addInfo)
    {
        Id = id;
        Name = name;
        VoiceType = voiceType;
        Gender = gender;
        BirthDate = birthDate;
        Height = height;
        Weight = weight;
        AddInfo = addInfo;
    }

    [JsonPropertyName("id")]
    public int Id { get; }
    
    [JsonPropertyName("name")]
    public string Name { get; }
    
    [JsonPropertyName("voiceType")]
    public VoiceType VoiceType { get; }
    
    [JsonPropertyName("gender")]
    public Gender Gender { get; }
    
    [JsonPropertyName("birthDate")]
    public DateTime BirthDate { get; }
    
    [JsonPropertyName("height")]
    public int Height { get; }
    
    [JsonPropertyName("weight")]
    public int Weight { get; }
    
    [JsonPropertyName("addInfo")]
    public string AddInfo { get; }
}