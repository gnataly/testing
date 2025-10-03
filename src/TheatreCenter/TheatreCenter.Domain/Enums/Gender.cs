using System.ComponentModel.DataAnnotations;
namespace TheatreCenter.Domain.Enums;
public enum Gender
{
    [Display(Name = "Муж.")]
    Male,
    [Display(Name = "Жен.")]
    Female
}

