
namespace TheatreCenter.Domain.Models
{
    public class ActorRole
    {
        public ActorRole(int actorId, int roleId)
        {
            ActorId = actorId;
            RoleId = roleId;
        }

        public int ActorId { get; set; }
        public Actor? Actor { get; set; }

        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
}