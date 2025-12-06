//namespace TheatreCenter.DTOs;
//using System.Text.Json.Serialization;
//using TheatreCenter.Domain.Enums;

//public class CreateActorDto
//{
//    public CreateActorDto(string name, VoiceType voiceType, Gender gender,
//                         DateTime birthDate, int height, int weight, string addInfo)
//    {
//        Name = name;
//        VoiceType = voiceType;
//        Gender = gender;
//        BirthDate = birthDate;
//        Height = height;
//        Weight = weight;
//        AddInfo = addInfo;
//    }

//    [JsonPropertyName("name")]
//    public string Name { get; }
    
//    [JsonPropertyName("voiceType")]
//    public VoiceType VoiceType { get; }
    
//    [JsonPropertyName("gender")]
//    public Gender Gender { get; }
    
//    [JsonPropertyName("birthDate")]
//    public DateTime BirthDate { get; }
    
//    [JsonPropertyName("height")]
//    public int Height { get; }
    
//    [JsonPropertyName("weight")]
//    public int Weight { get; }

//    [JsonPropertyName("addInfo")]
//    public string AddInfo { get; }
//}