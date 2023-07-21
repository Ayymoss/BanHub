using BanHubData.Enums;

namespace BanHub.WebCore.Shared.Models.PlayerProfileView;

public class Connection
{
    public string? ServerName { get; set; }
    public Game ServerGame { get; set; }
    public DateTimeOffset Connected { get; set; }
    public int ServerPort { get; set; }
    public string ServerIp { get; set; }
    public string CommunityIp { get; set; }
    public string? CommunityName { get; set; }
}
