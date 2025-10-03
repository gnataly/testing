namespace TheatreCenter.DTOs.Role;
using System.Text.Json.Serialization;
using TheatreCenter.Domain.Enums;

public class RoleDTO
{
    public RoleDTO(int id, string name, int musicalId, RoleType roleType)
    {
        Id = id;
        Name = name;
        MusicalId = musicalId;
        RoleType = roleType;
    }

    [JsonPropertyName("id")]
    public int Id { get; }
    
    [JsonPropertyName("name")]
    public string Name { get; }
    
    [JsonPropertyName("musicalId")]
    public int MusicalId { get; }
    
    [JsonPropertyName("roleType")]
    public RoleType RoleType { get; }
}