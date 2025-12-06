using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TheatreCenter.DTOs;

public class AddThemeToMusicalRequestDto
{
    [JsonPropertyName("musicalId")]
    public int MusicalId { get; set; }

    [JsonPropertyName("themeId")]
    public int ThemeId { get; set; }
}

public class AddActorToRoleRequestDto
{
    [JsonPropertyName("actorId")]
    public int ActorId { get; set; }

    [JsonPropertyName("roleId")]
    public int RoleId { get; set; }
}

public class ActorRoleInfoDto
{
    [JsonPropertyName("actorId")]
    public int ActorId { get; set; }

    [JsonPropertyName("roleId")]
    public int RoleId { get; set; }

    [JsonPropertyName("actorName")]
    public string ActorName { get; set; }

    [JsonPropertyName("roleName")]
    public string RoleName { get; set; }

    [JsonPropertyName("musicalName")]
    public string MusicalName { get; set; }
}

public class MusicalThemeInfoDto
{
    [JsonPropertyName("musicalId")]
    public int MusicalId { get; set; }

    [JsonPropertyName("themeId")]
    public int ThemeId { get; set; }

    [JsonPropertyName("musicalTitle")]
    public string MusicalTitle { get; set; }

    [JsonPropertyName("themeName")]
    public string ThemeName { get; set; }
}
