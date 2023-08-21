namespace BanHub.WebCore.Shared.Models.PlayersView;

public class Player
{
    public string Identity { get; set; }
    public string UserName { get; set; }
    public int Penalties { get; set; }
    public bool IsOnline { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Heartbeat { get; set; }
}
