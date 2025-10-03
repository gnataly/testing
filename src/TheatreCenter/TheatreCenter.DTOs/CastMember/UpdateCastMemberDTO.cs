using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.CastMember
{
    public class UpdateCastMemberDTO
    {
        public UpdateCastMemberDTO(int roleId, int actorId, string comment)
        {
            RoleId = roleId;
            ActorId = actorId;
            Comment = comment;
        }

        [JsonPropertyName("roleId")]
        public int RoleId { get; }

        [JsonPropertyName("actorId")]
        public int ActorId { get; }

        [JsonPropertyName("comment")]
        public string Comment { get; }
    }
}
