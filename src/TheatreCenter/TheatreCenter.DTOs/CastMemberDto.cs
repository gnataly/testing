using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs;

public class CastMemberDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("showId")]
    public int ShowId { get; set; }

    [JsonPropertyName("roleId")]
    public int RoleId { get; set; }

    [JsonPropertyName("actorId")]
    public int ActorId { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; }
}

public class CastMemberListDto
{
    [JsonPropertyName("items")]
    public List<CastMemberDto> Items { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationDto Pagination { get; set; }
}

public class CreateCastMemberRequestDto
{
    [JsonPropertyName("showId")]
    public int ShowId { get; set; }

    [JsonPropertyName("roleId")]
    public int RoleId { get; set; }

    [JsonPropertyName("actorId")]
    public int ActorId { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; }
}

public class UpdateCastMemberRequestDto
{
    [JsonPropertyName("roleId")]
    public int RoleId { get; set; }

    [JsonPropertyName("actorId")]
    public int ActorId { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; }
}
