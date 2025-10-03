using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheatreCenter.Domain.Models
{
    public class Theatre : IValidatableObject
    {
        public Theatre(int id, string name, string addInfo)
        {
            Id = id;
            Name = name;
            AddInfo = addInfo;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название театра обязательно")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Название должно быть от 2 до 100 символов")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Доп. информация не должна превышать 1000 символов")]
        public string AddInfo { get; set; }

        public ICollection<Musical> Musicals { get; set; } = new List<Musical>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Name.Contains("  "))
            {
                yield return new ValidationResult("Название содержит двойные пробелы",
                    new[] { nameof(Name) });
            }
        }
    }
}