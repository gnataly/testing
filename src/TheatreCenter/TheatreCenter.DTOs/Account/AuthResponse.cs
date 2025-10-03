using TheatreCenter.Domain.Enums;

namespace TheatreCenter.DTOs.Account
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public AccountDTO Account { get; set; }
    }
}
    