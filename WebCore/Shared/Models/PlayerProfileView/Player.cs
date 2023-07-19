namespace BanHub.WebCore.Shared.Models.PlayerProfileView;

public class Player
{
    public string Identity { get; set; }
    public string UserName { get; set; }
    public DateTimeOffset HeartBeat { get; set; }
    public bool IsGloballyBanned { get; set; }
    public int TotalConnections { get; set; }
    public TimeSpan PlayTime { get; set; }
    public DateTimeOffset Created { get; set; }
    public bool Connected { get; set; }
}
