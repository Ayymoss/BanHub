namespace BanHub.WebCore.Shared.Models.PlayersView;

public class PlayerContext
{
    public List<Player> Players { get; set; }
    public int Count { get; set; }
}

public class Player
{
    public string Identity { get; set; }
    public string UserName { get; set; }
    public int Penalties { get; set; }
    public DateTimeOffset HeartBeat { get; set; }
    public DateTimeOffset Created { get; set; }
}
