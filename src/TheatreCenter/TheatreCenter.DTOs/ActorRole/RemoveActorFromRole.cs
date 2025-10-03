using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.ActorRoles
{
    public class RemoveActorFromRoleDTO
    {
        public RemoveActorFromRoleDTO(int actorId, int roleId)
        {
            ActorId = actorId;
            RoleId = roleId;
        }

        [JsonPropertyName("actorId")]
        public int ActorId { get; }

        [JsonPropertyName("roleId")]
        public int RoleId { get; }
    }
}