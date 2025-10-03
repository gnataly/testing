using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.Theatre;

public class CreateTheatreDTO
{
    public CreateTheatreDTO(string name, string addInfo)
    {
        Name = name;
        AddInfo = addInfo;
    }

    [JsonPropertyName("name")]
    public string Name { get; }
    
    [JsonPropertyName("addInfo")]
    public string AddInfo { get; }
}