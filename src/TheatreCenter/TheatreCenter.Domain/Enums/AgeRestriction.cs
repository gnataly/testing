using System.ComponentModel.DataAnnotations;
namespace TheatreCenter.Domain.Enums;

public enum AgeRestriction
{
    [Display(Name = "0+")]
    AllAges,
    
    [Display(Name = "6+")]
    SixPlus,
    
    [Display(Name = "12+")]
    TwelvePlus,
    
    [Display(Name = "16+")]
    SixteenPlus,
    
    [Display(Name = "18+")]
    EighteenPlus
}
