using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Domain.Models
{
    public class Actor : IValidatableObject
    {
        public Actor(int id, string name, VoiceType voiceType, Gender gender,
                    DateTime birthDate, int height, int weight, string addInfo)
        {
            Id = id;
            Name = name;
            VoiceType = voiceType;
            Gender = gender;
            BirthDate = birthDate;
            Height = height;
            Weight = weight;
            AddInfo = addInfo;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя актера обязательно")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 100 символов")]
        public string Name { get; set; }

        public VoiceType VoiceType { get; set; }

        [Required(ErrorMessage = "Пол актера обязателен")]
        public Gender Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Range(100, 250, ErrorMessage = "Рост должен быть между 100 и 250 см")]
        public int Height { get; set; }

        [Range(30, 300, ErrorMessage = "Вес должен быть между 30 и 300 кг")]
        public int Weight { get; set; }

        [StringLength(500, ErrorMessage = "Дополнительная информация не должна превышать 500 символов")]
        public string AddInfo { get; set; }

        [NotMapped]
        public bool IsFavorite { get; set; }

        public ICollection<ActorRole> ActorRoles { get; set; } = new List<ActorRole>();
        public ICollection<CastMember> CastMembers { get; set; } = new List<CastMember>();


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Валидация типа голоса
            var maleVoices = new[] { VoiceType.Tenor, VoiceType.Baritone, VoiceType.Bass };
            var femaleVoices = new[] { VoiceType.Soprano, VoiceType.MezzoSoprano, VoiceType.Contralto };

            if (Gender == Gender.Male && !maleVoices.Contains(VoiceType))
            {
                yield return new ValidationResult(
                    "Для мужского пола допустимы только тенор, баритон или бас",
                    new[] { nameof(VoiceType) });
            }

            if (Gender == Gender.Female && !femaleVoices.Contains(VoiceType))
            {
                yield return new ValidationResult(
                    "Для женского пола допустимы только сопрано, меццо-сопрано или контральто",
                    new[] { nameof(VoiceType) });
            }

            // Валидация даты рождения
            if (BirthDate > DateTime.Now)
            {
                yield return new ValidationResult(
                    "Дата рождения не может быть в будущем",
                    new[] { nameof(BirthDate) });
            }

            if (BirthDate < DateTime.Now.AddYears(-120))
            {
                yield return new ValidationResult(
                    "Дата рождения слишком далеко в прошлом",
                    new[] { nameof(BirthDate) });
            }
        }
    }
}