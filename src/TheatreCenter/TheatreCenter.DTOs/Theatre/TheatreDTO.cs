using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.Theatre;

public class TheatreDTO
{
    public TheatreDTO(int id, string name, string addInfo)
    {
        Id = id;
        Name = name;
        AddInfo = addInfo;
    }

    [JsonPropertyName("id")]
    public int Id { get; }
    
    [JsonPropertyName("name")]
    public string Name { get; }
    
    [JsonPropertyName("addInfo")]
    public string AddInfo { get; }
}