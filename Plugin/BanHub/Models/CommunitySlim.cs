using BanHubData.Commands.Chat;

namespace BanHub.Models;

public class CommunitySlim
{
    public Dictionary<string, List<MessageContext>> PlayerMessages { get; set; } = new();
    public Guid CommunityGuid { get; set; }
    public string CommunityIp { get; set; }
    public Guid ApiKey { get; set; }
    public bool Active { get; set; }
}

