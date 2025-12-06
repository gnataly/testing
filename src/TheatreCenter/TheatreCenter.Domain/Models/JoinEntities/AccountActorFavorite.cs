
using TheatreCenter.Domain.Models;

public class AccountActorFavorite
{
    public AccountActorFavorite(int accountId, int actorId)
    {
        AccountId = accountId;
        ActorId = actorId;
        AddedDate = DateTime.UtcNow;
    }

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public int ActorId { get; set; }
    public Actor? Actor { get; set; }

    public DateTime AddedDate { get; set; }
}
