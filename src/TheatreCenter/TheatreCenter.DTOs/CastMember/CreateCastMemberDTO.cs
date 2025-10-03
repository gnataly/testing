using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.CastMember
{
    public class CreateCastMemberDTO
    {
        public CreateCastMemberDTO(int showId, int roleId, int actorId, string comment)
        {
            ShowId = showId;
            RoleId = roleId;
            ActorId = actorId;
            Comment = comment;
        }

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
