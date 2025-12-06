namespace TheatreCenter.DTOs;

using System.Text.Json.Serialization;
using TheatreCenter.Domain.Enums;

public class RoleDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("musicalId")]
    public int MusicalId { get; set; }

    [JsonPropertyName("roleType")]
    public RoleType RoleType { get; set; }
}

public class RoleListDto
{
    [JsonPropertyName("items")]
    public List<RoleDto> Items { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationDto Pagination { get; set; }
}

public class CreateRoleRequestDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("musicalId")]
    public int MusicalId { get; set; }

    [JsonPropertyName("roleType")]
    public RoleType RoleType { get; set; }
}

public class UpdateRoleRequestDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("roleType")]
    public RoleType RoleType { get; set; }
}
