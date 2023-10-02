using BanHub.Domain.Enums;

namespace BanHub.Domain.ValueObjects.Repository.Server;

public class ServerPaginationView
{
    public string ServerIp { get; set; }
    public string CommunityIp { get; set; }
    public Game ServerGame { get; set; }
    public int ServerPort { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string ServerName { get; set; }
    public Guid CommunityGuid { get; set; }
    public int MaxPlayers { get; set; }
    public string CommunityName { get; set; }
}
