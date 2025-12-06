using System.ComponentModel.DataAnnotations;
namespace TheatreCenter.Domain.Enums;

public enum VoiceType
{
    [Display(Name = "Тенор")]
    Tenor,

    [Display(Name = "Баритон")]
    Baritone,

    [Display(Name = "Бас")]
    Bass,

    [Display(Name = "Сопрано")]
    Soprano,

    [Display(Name = "Меццо-сопрано")]
    MezzoSoprano,

    [Display(Name = "Контральто")]
    Contralto
}
