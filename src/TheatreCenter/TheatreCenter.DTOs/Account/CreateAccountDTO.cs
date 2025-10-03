using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.Account
{
    public class CreateAccountDTO
    {
        public CreateAccountDTO(string username, string passwordHash)
        {
            Username = username;
            PasswordHash = passwordHash;
        }

        [JsonPropertyName("username")]
        public string Username { get; }

        [JsonPropertyName("passwordHash")]
        public string PasswordHash { get; }
    }
}