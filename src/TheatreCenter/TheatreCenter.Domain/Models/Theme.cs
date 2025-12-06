using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheatreCenter.Domain.Models
{
    public class Theme : IValidatableObject
    {
        public Theme(int id, string name)
        {
            Id = id;
            Name = name;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название темы обязательно")]
        [StringLength(50, ErrorMessage = "Название темы не должно превышать 50 символов")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я\s]+$", ErrorMessage = "Только буквы и пробелы")]
        public string Name { get; set; }

        public ICollection<MusicalTheme> MusicalThemes { get; set; } = new List<MusicalTheme>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Name.StartsWith(" ") || Name.EndsWith(" "))
            {
                yield return new ValidationResult("Название темы не должно начинаться или заканчиваться пробелом",
                    new[] { nameof(Name) });
            }
        }
    }
}
