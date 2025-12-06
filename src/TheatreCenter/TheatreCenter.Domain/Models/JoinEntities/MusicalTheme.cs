namespace TheatreCenter.Domain.Models
{
    public class MusicalTheme
    {
        public MusicalTheme(int musicalId, int themeId)
        {
            MusicalId = musicalId;
            ThemeId = themeId;
        }

        public int MusicalId { get; set; }
        public Musical? Musical { get; set; }

        public int ThemeId { get; set; }
        public Theme? Theme { get; set; }
    }
}
