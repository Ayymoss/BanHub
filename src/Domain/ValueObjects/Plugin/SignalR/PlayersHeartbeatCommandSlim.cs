namespace BanHub.Domain.ValueObjects.Plugin.SignalR;

public class PlayersHeartbeatCommandSlim
{
    public Guid CommunityGuid { get; set; }
    public List<string> PlayerIdentities { get; set; }
}
