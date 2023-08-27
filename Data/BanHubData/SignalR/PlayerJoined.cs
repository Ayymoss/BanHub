namespace BanHubData.SignalR;

public class PlayerJoined
{
    public Version PluginVersion { get; set; }
    public Guid CommunityApiKey { get; set; }
    public string Identity { get; set; }
}
