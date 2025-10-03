using System.ComponentModel.DataAnnotations;
namespace TheatreCenter.Domain.Enums;

public enum RoleType
{
    [Display(Name = "Главная")]
    Main,
    
    [Display(Name = "Второстепенная")]
    Supporting,
    
    [Display(Name = "Ансамбль")]
    Ensemble
}