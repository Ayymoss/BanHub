using BanHubData.Enums;

namespace BanHub.WebCore.Shared.Models.CommunityProfileView;

public class Server
{
    public string ServerName { get; set; }
    public string ServerIp { get; set; }
    public int ServerPort { get; set; }
    public Game ServerGame { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string ServerId { get; set; }
}
