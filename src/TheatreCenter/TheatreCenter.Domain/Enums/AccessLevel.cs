using System.ComponentModel.DataAnnotations;

namespace TheatreCenter.Domain.Enums
{
    public enum AccessLevel
    {
        [Display(Name = "Администратор")]
        Admin,

        [Display(Name = "Пользователь")]
        User
    }
}
