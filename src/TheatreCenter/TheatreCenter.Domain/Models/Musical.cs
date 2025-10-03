using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Domain.Models
{
    public class Musical : IValidatableObject
    {
        public Musical(int id, string title, string description, TimeSpan duration,
                      AgeRestriction ageRestriction, int theatreId)
        {
            Id = id;
            Title = title;
            Description = description;
            Duration = duration;
            AgeRestriction = ageRestriction;
            TheatreId = theatreId;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название мюзикла обязательно")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Название должно быть от 2 до 100 символов")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Описание обязательно")]
        [StringLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов")]
        public string Description { get; set; }

        [Range(typeof(TimeSpan), "00:30:00", "06:00:00", ErrorMessage = "Длительность должна быть от 30 минут до 6 часов")]
        public TimeSpan Duration { get; set; }

        [Required(ErrorMessage = "Возрастное ограничение обязательно")]
        public AgeRestriction AgeRestriction { get; set; }

        [Required(ErrorMessage = "ID театра обязательно")]
        public int TheatreId { get; set; }

        //public DateTime UpdatedAt { get; set; }

        public Theatre? Theatre { get; set; }
        public ICollection<Show> Shows { get; set; } = new List<Show>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<MusicalTheme> MusicalThemes { get; set; } = new List<MusicalTheme>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Проверка длительности для детских мюзиклов
            if ((AgeRestriction == AgeRestriction.AllAges ||
                 AgeRestriction == AgeRestriction.SixPlus) &&
                Duration > TimeSpan.FromHours(1.5))
            {
                yield return new ValidationResult(
                    "Мюзиклы для детей (0+ и 6+) не должны длиться более 1.5 часов",
                    new[] { nameof(Duration), nameof(AgeRestriction) });
            }

            // Проверка на соответствие возраста и содержания
            if (AgeRestriction == AgeRestriction.AllAges &&
                (Description.Contains("насилие") || Description.Contains("смерть")))
            {
                yield return new ValidationResult(
                    "Мюзиклы 0+ не могут содержать сцены насилия или смерти",
                    new[] { nameof(Description), nameof(AgeRestriction) });
            }

            // Проверка длительности для подростковых мюзиклов
            if (AgeRestriction == AgeRestriction.TwelvePlus &&
                Duration > TimeSpan.FromHours(2.5))
            {
                yield return new ValidationResult(
                    "Мюзиклы 12+ не должны длиться более 2.5 часов",
                    new[] { nameof(Duration), nameof(AgeRestriction) });
            }
        }
    }
}