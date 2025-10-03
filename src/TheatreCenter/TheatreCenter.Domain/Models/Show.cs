using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheatreCenter.Domain.Models
{
    public class Show : IValidatableObject
    {
        public Show(int id, DateTime date, int musicalId)
        {
            Id = id;
            Date = date;
            MusicalId = musicalId;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Дата показа обязательна")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "ID мюзикла обязательно")]
        public int MusicalId { get; set; }

        public Musical? Musical { get; set; }
        public ICollection<CastMember> CastMembers { get; set; } = new List<CastMember>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Date < DateTime.Now.AddYears(-1))
            {
                yield return new ValidationResult("Дата показа не может быть более чем год назад",
                    new[] { nameof(Date) });
            }

            if (Date > DateTime.Now.AddYears(2))
            {
                yield return new ValidationResult("Дата показа не может быть более чем через 2 года",
                    new[] { nameof(Date) });
            }

            if (Date.DayOfWeek == DayOfWeek.Monday)
            {
                yield return new ValidationResult("Нельзя назначать показы на понедельник",
                    new[] { nameof(Date) });
            }
        }
    }
}