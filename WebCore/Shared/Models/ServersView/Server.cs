using BanHubData.Enums;

namespace BanHub.WebCore.Shared.Models.ServersView;

public class Server
{
    public string ServerName { get; set; }
    public string ServerIp { get; set; }
    public int ServerPort { get; set; }
    public Game ServerGame { get; set; }
    public DateTimeOffset Updated { get; set; }
    public Guid CommunityGuid { get; set; }
    public string CommunityName { get; set; }
    public int MaxPlayers { get; set; }
    public int OnlineCount { get; set; }
}
