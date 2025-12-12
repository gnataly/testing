//using System.Text.Json.Serialization;
//using System.Collections.Generic;

//namespace TheatreCenter.DTOs;

//public class AccountFavoritesDto
//{
//    public AccountFavoritesDto(
//        IEnumerable<FavoriteDto> favoriteActors,
//        IEnumerable<FavoriteDto> favoriteMusicals,
//        IEnumerable<FavoriteDto> favoriteTheatres)
//    {
//        FavoriteActors = favoriteActors;
//        FavoriteMusicals = favoriteMusicals;
//        FavoriteTheatres = favoriteTheatres;
//    }

//    [JsonPropertyName("favoriteActors")]
//    public IEnumerable<FavoriteDto> FavoriteActors { get; }

//    [JsonPropertyName("favoriteMusicals")]
//    public IEnumerable<FavoriteDto> FavoriteMusicals { get; }

//    [JsonPropertyName("favoriteTheatres")]
//    public IEnumerable<FavoriteDto> FavoriteTheatres { get; }
//}
