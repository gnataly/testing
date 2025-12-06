using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.Domain.Models
{
    public class Account : IValidatableObject
    {
        public Account(int id, string username, string passwordHash, AccessLevel accessLevel)
        {
            Id = id;
            Username = username;
            PasswordHash = passwordHash;
            LastFavoritesViewDate = DateTime.UtcNow;
            AccessLevel = accessLevel;
        }

        public Account(int id, string username, string passwordHash, DateTime lastFavoritesViewDate, AccessLevel accessLevel)
        {
            Id = id;
            Username = username;
            PasswordHash = passwordHash;
            LastFavoritesViewDate = lastFavoritesViewDate;
            AccessLevel = accessLevel;
        }

        public Account(int id, string username, string passwordHash)
        {
            Id = id;
            Username = username;
            PasswordHash = passwordHash;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Только буквы, цифры и подчёркивания")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Хэш пароля обязателен")]
        [StringLength(255, ErrorMessage = "Хэш пароля слишком длинный")]
        public string PasswordHash { get; set; }

        [Required]
        public AccessLevel AccessLevel { get; set; } = AccessLevel.User;


        [DataType(DataType.DateTime)]
        public DateTime LastFavoritesViewDate { get; set; } = DateTime.UtcNow;

        [Required]
        public bool UpgradeRequest { get; set; } = false;

        // Навигационные свойства для избранного
        public ICollection<AccountTheatreFavorite> FavoriteTheatres { get; set; } = new List<AccountTheatreFavorite>();
        public ICollection<AccountMusicalFavorite> FavoriteMusicals { get; set; } = new List<AccountMusicalFavorite>();
        public ICollection<AccountActorFavorite> FavoriteActors { get; set; } = new List<AccountActorFavorite>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (LastFavoritesViewDate > DateTime.UtcNow.AddDays(1))
            {
                yield return new ValidationResult("Дата просмотра не может быть в будущем",
                    new[] { nameof(LastFavoritesViewDate) });
            }
        }

        public void UpdateLastViewDate() => LastFavoritesViewDate = DateTime.UtcNow;

        public void ChangePassword(string newHash)
        {
            if (string.IsNullOrWhiteSpace(newHash) || newHash.Length > 255)
                throw new ArgumentException("Некорректный хэш пароля");

            PasswordHash = newHash;
        }
    }
}
