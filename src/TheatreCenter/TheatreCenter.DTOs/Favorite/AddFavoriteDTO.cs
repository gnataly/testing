
using System.Text.Json.Serialization;
namespace TheatreCenter.DTOs.Favorite;
public class AddFavoriteDTO
{
    public AddFavoriteDTO(int accountId, int targetId)
    {
        AccountId = accountId;
        TargetId = targetId;
    }

    [JsonPropertyName("accountId")]
    public int AccountId { get; }

    [JsonPropertyName("targetId")]
    public int TargetId { get; }
}

