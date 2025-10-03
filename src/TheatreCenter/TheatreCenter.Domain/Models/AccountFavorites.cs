namespace TheatreCenter.Domain.Models
{
    public class AccountFavorites
    {
        public List<Actor> Actors { get; set; } = new List<Actor>();
        public List<Musical> Musicals { get; set; } = new List<Musical>();
        public List<Theatre> Theatres { get; set; } = new List<Theatre>();
    }
}
