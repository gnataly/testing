using System.Text.Json.Serialization;
using System;
using TheatreCenter.Domain.Enums;

namespace TheatreCenter.DTOs;

public class AccountDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("lastFavoritesViewDate")]
    public DateTime LastFavoritesViewDate { get; set; }

    [JsonPropertyName("accessLevel")]
    public AccessLevel AccessLevel { get; set; }

    [JsonPropertyName("upgradeRequest")]
    public bool UpgradeRequest { get; set; }

    [JsonPropertyName("lockedUntil")]
    public DateTime? LockedUntil { get; set; }

    [JsonPropertyName("lastPasswordChangeAt")]
    public DateTime? LastPasswordChangeAt { get; set; }
}

public class AccountListDto
{
    [JsonPropertyName("items")]
    public List<AccountDto> Items { get; set; }

    [JsonPropertyName("pagination")]
    public PaginationDto Pagination { get; set; }
}

public class UpdateAccountRequestDto
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("passwordHash")]
    public string PasswordHash { get; set; }
}

public class UpgradeRequestDto
{
    [JsonPropertyName("isApproved")]
    public bool IsApproved { get; set; }
}
