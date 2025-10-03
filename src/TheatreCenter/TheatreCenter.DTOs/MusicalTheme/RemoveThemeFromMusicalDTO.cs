using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs.MusicalThemes
{
    public class RemoveThemeFromMusicalDTO
    {
        public RemoveThemeFromMusicalDTO(int musicalId, int themeId)
        {
            MusicalId = musicalId;
            ThemeId = themeId;
        }

        [JsonPropertyName("musicalId")]
        public int MusicalId { get; }

        [JsonPropertyName("themeId")]
        public int ThemeId { get; }
    }
}