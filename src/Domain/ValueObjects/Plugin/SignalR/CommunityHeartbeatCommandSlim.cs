namespace BanHub.Domain.ValueObjects.Plugin.SignalR;

public class CommunityHeartbeatCommandSlim
{
    public Guid ApiKey { get; set; }
    public Guid CommunityGuid { get; set; }
}
