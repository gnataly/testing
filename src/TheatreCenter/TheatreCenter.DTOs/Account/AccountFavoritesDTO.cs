using System.Text.Json.Serialization;
using System.Collections.Generic;
using TheatreCenter.DTOs.Favorite;

namespace TheatreCenter.DTOs.Account
{
    public class AccountFavoritesDTO
    {
        public AccountFavoritesDTO(
            IEnumerable<FavoriteItemDTO> favoriteActors,
            IEnumerable<FavoriteItemDTO> favoriteMusicals,
            IEnumerable<FavoriteItemDTO> favoriteTheatres)
        {
            FavoriteActors = favoriteActors;
            FavoriteMusicals = favoriteMusicals;
            FavoriteTheatres = favoriteTheatres;
        }

        [JsonPropertyName("favoriteActors")]
        public IEnumerable<FavoriteItemDTO> FavoriteActors { get; }

        [JsonPropertyName("favoriteMusicals")]
        public IEnumerable<FavoriteItemDTO> FavoriteMusicals { get; }

        [JsonPropertyName("favoriteTheatres")]
        public IEnumerable<FavoriteItemDTO> FavoriteTheatres { get; }
    }
}