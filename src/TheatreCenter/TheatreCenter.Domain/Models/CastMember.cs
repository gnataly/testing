using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheatreCenter.Domain.Models;

namespace TheatreCenter.Domain.Models
{
    public class CastMember : IValidatableObject
    {
        public CastMember(int id, int showId, int roleId, int actorId, string comment)
        {
            Id = id;
            ShowId = showId;
            RoleId = roleId;
            ActorId = actorId;
            Comment = comment;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID показа обязательно")]
        public int ShowId { get; set; }

        [Required(ErrorMessage = "ID роли обязательно")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "ID актера обязательно")]
        public int ActorId { get; set; }

        [StringLength(200, MinimumLength = 0, ErrorMessage = "Комментарий должен быть до 200 символов")]
        public string Comment { get; set; }

        public Show? Show { get; set; }
        public Role? Role { get; set; }
        public Actor? Actor { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ShowId <= 0 || RoleId <= 0 || ActorId <= 0)
            {
                yield return new ValidationResult("ID должны быть положительными числами",
                    new[] { nameof(ShowId), nameof(RoleId), nameof(ActorId) });
            }
        }
    }
}
