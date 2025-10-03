using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.ActorRoles
{
    public class ActorRoleDTO
    {
        public ActorRoleDTO(int actorId, int roleId, string actorName, string roleName)
        {
            ActorId = actorId;
            RoleId = roleId;
            ActorName = actorName;
            RoleName = roleName;
        }

        [JsonPropertyName("actorId")]
        public int ActorId { get; }

        [JsonPropertyName("roleId")]
        public int RoleId { get; }

        [JsonPropertyName("actorName")]
        public string ActorName { get; }

        [JsonPropertyName("roleName")]
        public string RoleName { get; }
    }
}