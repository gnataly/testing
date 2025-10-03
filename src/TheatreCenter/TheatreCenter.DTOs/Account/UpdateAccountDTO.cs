using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.Account
{
    public class UpdateAccountDTO
    {
        public UpdateAccountDTO(string username)
        {
            Username = username;
        }

        [JsonPropertyName("username")]
        public string Username { get; }
    }
}