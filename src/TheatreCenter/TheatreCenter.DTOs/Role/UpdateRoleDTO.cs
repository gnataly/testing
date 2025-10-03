namespace TheatreCenter.DTOs.Role;
using System.Text.Json.Serialization;
using TheatreCenter.Domain.Enums;

public class UpdateRoleDTO
{
    public UpdateRoleDTO(RoleType roleType)
    {
        RoleType = roleType;
    }

    [JsonPropertyName("roleType")]
    public RoleType RoleType { get; }
}