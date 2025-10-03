using System.Text.Json.Serialization;
using System;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.DTOs.Account
{
    public class AccountDTO
    {
        public AccountDTO(int id, string username, DateTime lastFavoritesViewDate, AccessLevel accessLevel, bool upgradeRequest)
        {
            Id = id;
            Username = username;
            LastFavoritesViewDate = lastFavoritesViewDate;
            AccessLevel = accessLevel;
            UpgradeRequest = upgradeRequest;
        }

        [JsonPropertyName("id")]
        public int Id { get; }

        [JsonPropertyName("username")]
        public string Username { get; }

        [JsonPropertyName("lastFavoritesViewDate")]
        public DateTime LastFavoritesViewDate { get; }

        [JsonPropertyName("accessLevel")]
        public AccessLevel AccessLevel { get; }

        [JsonPropertyName("upgradeRequest")]
        public bool UpgradeRequest { get; }
    }
}