using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.MusicalThemes
{
    public class MusicalThemeDTO
    {
        public MusicalThemeDTO(int musicalId, int themeId, string musicalTitle, string themeName)
        {
            MusicalId = musicalId;
            ThemeId = themeId;
            MusicalTitle = musicalTitle;
            ThemeName = themeName;
        }

        [JsonPropertyName("musicalId")]
        public int MusicalId { get; }

        [JsonPropertyName("themeId")]
        public int ThemeId { get; }

        [JsonPropertyName("musicalTitle")]
        public string MusicalTitle { get; }

        [JsonPropertyName("themeName")]
        public string ThemeName { get; }
    }
}