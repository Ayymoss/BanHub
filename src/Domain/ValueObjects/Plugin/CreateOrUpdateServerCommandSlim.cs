using BanHub.Domain.Enums;

namespace BanHub.Domain.ValueObjects.Plugin;

public class CreateOrUpdateServerCommandSlim
{
    public string ServerName { get; set; }
    public Game ServerGame { get; set; }
    public string ServerId { get; set; }
    public string ServerIp { get; set; }
    public int ServerPort { get; set; }
    public Guid CommunityGuid { get; set; }
}
