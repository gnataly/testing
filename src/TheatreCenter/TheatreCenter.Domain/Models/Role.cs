using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Domain.Models
{
    public class Role : IValidatableObject
    {
        public Role(int id, string name, int musicalId, RoleType roleType)
        {
            Id = id;
            Name = name;
            MusicalId = musicalId;
            RoleType = roleType;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название роли обязательно")]
        [StringLength(50, ErrorMessage = "Название роли не должно превышать 50 символов")]
        public string Name { get; set; }

        [Required(ErrorMessage = "ID мюзикла обязательно")]
        public int MusicalId { get; set; }

        public Musical? Musical { get; set; }

        [Required(ErrorMessage = "Тип роли обязателен")]
        public RoleType RoleType { get; set; }

        public ICollection<ActorRole> ActorRoles { get; set; } = new List<ActorRole>();
        public ICollection<CastMember> CastMembers { get; set; } = new List<CastMember>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (RoleType == RoleType.Main && Name.Length > 30)
            {
                yield return new ValidationResult("Название главной роли должно быть короче 30 символов",
                    new[] { nameof(Name) });
            }
        }
    }
}
