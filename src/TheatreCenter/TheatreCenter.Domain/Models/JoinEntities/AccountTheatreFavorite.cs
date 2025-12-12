
using TheatreCenter.Domain.Models;

public class AccountTheatreFavorite
{
    public AccountTheatreFavorite(int accountId, int theatreId)
    {
        AccountId = accountId;
        TheatreId = theatreId;
        AddedDate = DateTime.UtcNow;
    }

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public int TheatreId { get; set; }
    public Theatre? Theatre { get; set; }

    public DateTime AddedDate { get; set; }
}
