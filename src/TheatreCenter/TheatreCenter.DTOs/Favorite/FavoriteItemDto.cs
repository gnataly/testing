using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.Favorite
{
    public class FavoriteItemDTO
    {
        public FavoriteItemDTO(int id, string name, DateTime lastModified)
        {
            Id = id;
            Name = name;
            lastModified = lastModified;
        }

        [JsonPropertyName("id")]
        public int Id { get; }

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("lastModified")]
        public DateTime LastModified { get; }
    }
}
