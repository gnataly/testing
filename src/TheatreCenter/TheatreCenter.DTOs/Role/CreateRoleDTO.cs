namespace TheatreCenter.DTOs.Role;
using System.Text.Json.Serialization;
using TheatreCenter.Domain.Enums;

public class CreateRoleDTO
{
    public CreateRoleDTO(string name, int musicalId, RoleType roleType)
    {
        Name = name;
        MusicalId = musicalId;
        RoleType = roleType;
    }

    [JsonPropertyName("name")]
    public string Name { get; }
    
    [JsonPropertyName("musicalId")]
    public int MusicalId { get; }
    
    [JsonPropertyName("roleType")]
    public RoleType RoleType { get; }
}