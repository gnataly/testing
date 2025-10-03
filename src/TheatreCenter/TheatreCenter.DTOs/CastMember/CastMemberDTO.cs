using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.CastMember
{
    public class CastMemberDTO
    {
        public CastMemberDTO(int id, int showId, int roleId, int actorId, string comment)
        {
            Id = id;
            ShowId = showId;
            RoleId = roleId;
            ActorId = actorId;
            Comment = comment;
        }

        [JsonPropertyName("id")]
        public int Id { get; }

        [JsonPropertyName("showId")]
        public int ShowId { get; }

        [JsonPropertyName("roleId")]
        public int RoleId { get; }

        [JsonPropertyName("actorId")]
        public int ActorId { get; }

        [JsonPropertyName("comment")]
        public string Comment { get; }
    }
}
