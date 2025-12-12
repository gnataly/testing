
using TheatreCenter.Domain.Models;

public class AccountMusicalFavorite
{
    public AccountMusicalFavorite(int accountId, int musicalId)
    {
        AccountId = accountId;
        MusicalId = musicalId;
        AddedDate = DateTime.UtcNow;
    }

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public int MusicalId { get; set; }
    public Musical? Musical { get; set; }

    public DateTime AddedDate { get; set; }
}
