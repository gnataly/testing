using System.Text.Json.Serialization;
using System;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.DTOs.Account
{
    public class AccountDTO
    {
        public AccountDTO(int id, string username, DateTime lastFavoritesViewDate, AccessLevel accessLevel)
        {
            Id = id;
            Username = username;
            LastFavoritesViewDate = lastFavoritesViewDate;
            AccessLevel = accessLevel;
        }

        [JsonPropertyName("id")]
        public int Id { get; }

        [JsonPropertyName("username")]
        public string Username { get; }

        [JsonPropertyName("lastFavoritesViewDate")]
        public DateTime LastFavoritesViewDate { get; }

        [JsonPropertyName("accessLevel")]
        public AccessLevel AccessLevel { get; }
    }
}