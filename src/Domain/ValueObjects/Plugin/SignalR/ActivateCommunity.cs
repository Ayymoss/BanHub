namespace BanHub.Domain.ValueObjects.Plugin.SignalR;

public class ActivateCommunity
{
    public bool Activated { get; set; }
    public Guid ApiKey { get; set; }
}
